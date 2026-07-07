git clone https://github.com/InsonusK/ai-skill-manager
Rename-Item -Path "ai-skill-manager" -NewName ".ai-skill-manager"
cd .ai-skill-manager
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -e .
cd ..