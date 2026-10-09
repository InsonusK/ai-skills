import {Then} from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import {version} from '../index';
import manifest from '../../../package.json';
Then("the package version equals the installed distribution's version",function(){this.log(`then: version=${version} distribution=${manifest.version}`);assert.equal(version,manifest.version);});
