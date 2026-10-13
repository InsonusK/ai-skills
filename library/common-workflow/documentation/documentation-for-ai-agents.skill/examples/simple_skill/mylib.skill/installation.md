# Installation and access

## MUST
- Install mylib from PyPI.
  ```bash
  pip install mylib==1.2.3
  ```
- Import the two public entry points.
  ```python
  from mylib.core import process_data, fetch_records
  ```
- Requires Python 3.10 or later.

## SHOULD
- Verify the installation by printing the package version.
  ```python
  import mylib
  print(mylib.__version__)
  ```
