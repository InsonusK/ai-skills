import {Given, When, Then} from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import {check} from '../checker';
Given('the URL {string}', function(url: string) { this.url=url; this.log(`given: url=${url}`); });
When('I check the URL', function() { this.result=check(this.url); this.log(`when: check -> ${JSON.stringify(this.result)}`); });
Then('the check is valid', function() { this.log(`then: valid=${this.result.is_valid}`); assert.equal(this.result.is_valid,true); });
Then('the normalized URL is {string}', function(url:string) { this.log(`then: normalized=${this.result.normalized}`); assert.equal(this.result.normalized,url); });
Then('the check is invalid with error {string}', function(error:string) { this.log(`then: error=${this.result.error_code}`); assert.equal(this.result.is_valid,false); assert.equal(this.result.error_code,error); });
