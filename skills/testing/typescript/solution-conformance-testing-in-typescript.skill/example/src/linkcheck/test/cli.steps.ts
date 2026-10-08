import {When, Then, DataTable} from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import {main} from '../cli';
When('I run the command with {string}', function(args:string) { this.out=[];this.err=[];this.exit=main(args ? args.split(' ') : [],line=>this.out.push(line),line=>this.err.push(line));this.log(`when: exit=${this.exit} out=${JSON.stringify(this.out)} err=${JSON.stringify(this.err)}`); });
Then('the exit code is {int}', function(code:number) {this.log(`then: exit=${this.exit}`);assert.equal(this.exit,code);});
Then('the output is:',function(table:DataTable){this.log(`then: output=${JSON.stringify(this.out)}`);assert.deepEqual(this.out,table.rows().map(row=>row[0]));});
Then('the error output starts with {string}',function(prefix:string){this.log(`then: error=${this.err.join('\n')}`);assert.ok(this.err.join('\n').startsWith(prefix));});
