"""Rewrites the step texts of the HTTP entry point's step definitions (Go and
Python) to the shared scenarios' wording. Literal replacements, in order,
inside every *_test.go / *_test.py / conftest.py under api/http/ of the given
examples."""
import glob, sys
R = [
 ("the command answers", "the echo command answers"),
 ("the command is rejected with", "the echo command is rejected with"),
 ("the command fails with", "the echo command fails with"),
 ("the command panics", "the echo command crashes"),
 ("the command is wrapped with logging", "the echo command is wrapped with logging"),
 ("the command should have received", "the echo command should have received"),
 ("the command should not have been called", "the echo command should not have been called"),
 ("the command should have run with the request id", "the echo command should have run with the request id"),
 ("the context of the command should carry the request id", "the echo command should have run with the request id"),
 ("the docs are enabled", "the HTTP docs are enabled"),
 ("\" with the body '", "\" to the HTTP API with the body '"),
 ("\\\" with the body '", "\\\" to the HTTP API with the body '"),
 (") with the body '", ") to the HTTP API with the body '"),
 ("the response status should be", "the HTTP status should be"),
 ("the response body should be", "the HTTP response body should be"),
 ("the response body should mention", "the HTTP response body should mention"),
 ("the response should redirect to", "the HTTP response should redirect to"),
 ("the response should be the committed spec", "the HTTP response should be the embedded spec"),
 ("the response should be the embedded spec", "the HTTP response should be the embedded spec"),
 ("the response should carry", "the HTTP response should carry"),
 ("the error body should have code", "the HTTP error should have code"),
 ("the log should hold the error", "the HTTP log should hold the error"),
 ("the records should be, in order", "the HTTP records should be, in order"),
 ("the middlewares ", "the HTTP middlewares "),
 ("a middleware that rejects", "an HTTP middleware that rejects"),
 ("the listen port should be", "the HTTP listen port should be"),
 ("the base path should be", "the HTTP base path should be"),
 ("the docs should be", "the HTTP docs should be"),
 ("loading should fail mentioning", "loading the HTTP settings should fail mentioning"),
 ("the server is serving on a free port", "the HTTP server is serving on a free port"),
 ("the process is told to terminate", "the HTTP server is told to stop"),
 ("the server should stop", "the HTTP server should stop"),
 ("the port the server wants", "the port the HTTP server wants"),
 ("the server starts", "the HTTP server starts"),
]
for ex in sys.argv[1:]:
    files = [f for pat in ("**/api/http/**/*_test.go", "**/api/http/**/*_test.py", "**/api/http/**/conftest.py")
             for f in glob.glob(f"{ex}/{pat}", recursive=True) if "/.venv/" not in f]
    for f in files:
        s = open(f).read(); o = s
        for a, b in R:
            if b in s and a in b:  # already rewritten: guard against doubling
                s = s.replace(b, "\0")
                s = s.replace(a, b).replace("\0", b)
            else:
                s = s.replace(a, b)
        if s != o:
            open(f, "w").write(s); print("rewrote", f)
