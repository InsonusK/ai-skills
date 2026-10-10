import { Then, DataTable } from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import { check } from '../checker';
Then('a check result has the fields:', function (table: DataTable) {
  const fields = Object.keys(check('https://a.example'));
  this.log(`then: fields=${fields}`);
  assert.deepEqual(
    fields,
    table.rows().map((row) => row[0]),
  );
});
Then('the error code is empty', function () {
  this.log(`then: error=${this.result.error_code}`);
  assert.equal(this.result.error_code, '');
});
