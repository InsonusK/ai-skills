import {Given, When, Then, DataTable} from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import {fromRecord,toRecord,RecordError} from '../mapping';
Given('the record:',function(table:DataTable){this.record=Object.fromEntries(table.rows());this.log(`given: record=${JSON.stringify(this.record)}`);});
When('I map the result to a record and back',function(){this.mapped=fromRecord(toRecord(this.result));this.log(`when: mapped=${JSON.stringify(this.mapped)}`);});
When('I map the record to a result',function(){try{this.mapped=fromRecord(this.record);}catch(error){assert.ok(error instanceof RecordError);this.error=error.code;}this.log(`when: mapping error=${this.error}`);});
Then('the result is unchanged',function(){this.log(`then: mapped=${JSON.stringify(this.mapped)}`);assert.deepEqual(this.mapped,this.result);});
Then('the mapping fails with error {string}',function(error:string){this.log(`then: error=${this.error}`);assert.equal(this.error,error);});
