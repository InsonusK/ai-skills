"""create list_items

Revision ID: 0001
Revises:
Create Date: 2026-11-03
"""

from collections.abc import Sequence

import sqlalchemy as sa
from alembic import op

revision: str = "0001"
down_revision: str | None = None
branch_labels: str | Sequence[str] | None = None
depends_on: str | Sequence[str] | None = None


def upgrade() -> None:
    op.create_table(
        "list_items",
        sa.Column("id", sa.Integer(), sa.Identity(), primary_key=True),
        sa.Column("list_id", sa.String(), nullable=False),
        sa.Column("title", sa.String(), nullable=False),
        sa.Column("done", sa.Boolean(), nullable=False),
        sa.UniqueConstraint("list_id", "title", name="list_items_list_id_title_key"),
    )


def downgrade() -> None:
    op.drop_table("list_items")
