init:
	curl -fsSL https://raw.githubusercontent.com/InsonusK/go-ai-skill-manage/master/scripts/install.sh | sh
	aism sync

ai-skill-sync:
	aism sync

test-lib:
	bash ./test/test.sh