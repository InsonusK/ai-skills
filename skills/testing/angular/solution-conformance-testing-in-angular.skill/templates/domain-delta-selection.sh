FILES=$(git diff --relative --name-only --diff-filter=ACMR "$SINCE" -- ':(glob){DomainRoot}/**/*.ts' ':(exclude,glob){DomainRoot}/**/test/**' | paste -sd, -)
