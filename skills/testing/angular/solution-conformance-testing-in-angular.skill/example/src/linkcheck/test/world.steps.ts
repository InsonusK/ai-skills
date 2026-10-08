import {After, Before, setWorldConstructor, World} from '@cucumber/cucumber';
import {mkdtemp, rm} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
export class LinkWorld extends World { [key: string]: any; }
setWorldConstructor(LinkWorld);
Before(async function(this: LinkWorld) { this.directory = await mkdtemp(join(tmpdir(), 'linkcheck-')); });
After(async function(this: LinkWorld) { await rm(this.directory, {recursive:true, force:true}); });
