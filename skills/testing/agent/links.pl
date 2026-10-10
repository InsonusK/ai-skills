#!/usr/bin/perl
# Lists every wiki/markdown link in the given markdown files as TSV:
#   source <TAB> line <TAB> raw target <TAB> resolved path (repo-relative, "?" if unknown) <TAB> ok|broken|placeholder
# Run from the repository root:  perl skills/testing/agent/links.pl <file>...
# Links inside fenced code blocks are skipped (template examples, not real links).
use strict;
use warnings;
use File::Basename qw(dirname);

my %basenames;    # lazily built index for bare Obsidian links: basename -> exists

sub norm {
    my ($p) = @_;
    my @out;
    for my $s (split m{/}, $p) {
        next if $s eq '' || $s eq '.';
        if ($s eq '..') { pop @out; next; }
        push @out, $s;
    }
    return join '/', @out;
}

sub exists_as {
    my ($p) = @_;
    for my $c ($p, "$p.md") { return $c if -e $c; }
    return undef;
}

sub bare_index {
    return if %basenames;
    open my $fh, '-|', 'git', 'ls-files', 'skills' or die "git ls-files: $!";
    while (my $f = <$fh>) {
        chomp $f;
        my ($b) = $f =~ m{([^/]+)$};
        $basenames{$b} = $f;
        (my $noext = $b) =~ s/\.md$//;
        $basenames{$noext} //= $f;
    }
    close $fh;
    $basenames{''} = '';
}

sub resolve {
    my ($src, $t) = @_;
    my $cand;
    if ($t =~ m{^skills/}) {
        $cand = norm($t);
    } elsif ($t =~ m{^\.\.?/} || $t =~ m{/}) {
        $cand = norm(dirname($src) . '/' . $t);
    } else {
        my $local = exists_as(norm(dirname($src) . '/' . $t));
        return ($local, 'ok') if defined $local;
        bare_index();
        return ($basenames{$t}, 'ok') if exists $basenames{$t};
        return ('?', $t =~ /[{<]/ ? 'placeholder' : 'broken');
    }
    my $hit = exists_as($cand);
    return ($hit, 'ok') if defined $hit;
    # a path-style link may still be relative to the file even without ./
    return ($cand, $t =~ /[{<]/ && $t !~ m{^skills/} ? 'placeholder' : 'broken');
}

for my $src (@ARGV) {
    open my $fh, '<', $src or next;
    my ($fence, $n) = (0, 0);
    while (my $line = <$fh>) {
        $n++;
        if ($line =~ /^\s*(```|~~~)/) { $fence = !$fence; next; }
        next if $fence;
        my @targets;
        while ($line =~ /\[\[([^\]\|#]+?)\\?(?:#[^\]\|]*?)?\\?(?:\|[^\]]*)?\]\]/g) { push @targets, $1; }
        while ($line =~ /\]\(<?([^)\s>]+)>?\)/g) { push @targets, $1; }
        for my $raw (@targets) {
            my $t = $raw;
            next if $t =~ m{^(?:[a-z][a-z0-9+.-]*:|#|/)}i;
            $t =~ s/#.*$//;
            $t =~ s/%20/ /g;
            next if $t eq '';
            my ($res, $st) = resolve($src, $t);
            print join("\t", $src, $n, $raw, $res, $st), "\n";
        }
    }
    close $fh;
}
