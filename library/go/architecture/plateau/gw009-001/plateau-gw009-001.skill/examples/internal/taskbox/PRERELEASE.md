# Pre-release copy of taskbox-go

This directory is the TaskBox code proven here before the `taskbox-go` library existed (conformance feature green on PostgreSQL 18). Its source of truth is now the library repository (`https://github.com/InsonusK/taskbox-go`, mock until published), which starts from this code.

At `taskbox-go` v0.1.0: delete this directory, add the module to `go.mod`, and change the imports `…/internal/taskbox` and `…/internal/taskbox/pgstore` to the module's packages. Do not edit the code here meanwhile — change it in the library.
