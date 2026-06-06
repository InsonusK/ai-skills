#!/usr/bin/env python3
"""
EN: Search for *.skill.md files recursively, distinguishing between skill directories
    (folder name matches a skill file name) and flat skill folders.
RU: Рекурсивно искать файлы *.skill.md, различая skill директории
    (имя папки совпадает с именем skill-файла) и flat skill папки.
"""

from __future__ import annotations

import argparse
import logging
import os
import sys
from pathlib import Path

# EN: Directories to skip during traversal.
# RU: Директории, которые пропускаются при обходе.
EXCLUDED_DIRS = {".git", ".venv", "venv", "node_modules", "__pycache__", ".pytest_cache"}


def _get_skill_prefix(filename: str) -> str | None:
    """
    EN: Return the skill name prefix from a filename, or None if not a skill file.
    RU: Вернуть префикс имени skill из имени файла, или None, если это не skill-файл.
    """
    suffix = ".skill.md"
    if filename.endswith(suffix):
        return filename[: -len(suffix)]
    return None


def _is_skill_dir(directory: Path, files: list[str], case_sensitive: bool) -> bool:
    """
    EN: Check whether any *.skill.md file in the current directory has a prefix
        matching the directory name.
    RU: Проверить, есть ли в текущей директории файл *.skill.md,
        префикс которого совпадает с именем папки.
    """
    folder_name = directory.name
    for filename in files:
        prefix = _get_skill_prefix(filename)
        if prefix is None:
            continue
        if case_sensitive:
            if prefix == folder_name:
                return True
        else:
            if prefix.lower() == folder_name.lower():
                return True
    return False


def _collect_skills_in_subtree(directory: Path) -> list[Path]:
    """
    EN: Collect all *.skill.md files inside `directory` and its subdirectories.
    RU: Собрать все файлы *.skill.md внутри `directory` и её подпапок.
    """
    skills: list[Path] = []
    for root, _dirs, files in os.walk(directory):
        for filename in files:
            if filename.endswith(".skill.md"):
                skills.append(Path(root) / filename)
    return skills


def _assert_skill_dir(directory: Path, case_sensitive: bool) -> dict[str, str] | None:
    """
    EN: Assert that a skill directory (folder name matches a skill file) contains
        exactly one *.skill.md file in the whole subtree.
        Return the skill entry if valid, otherwise log an error and return None.
    RU: Убедиться, что skill-директория (имя папки совпадает с именем skill-файла)
        содержит ровно один файл *.skill.md во всём поддереве.
        Вернуть запись skill, если валидно, иначе залогировать ошибку и вернуть None.
    """
    skills = _collect_skills_in_subtree(directory)
    if len(skills) > 1:
        skills_list = "\n ".join(str(skill) for skill in skills)
        logging.error(
            f"SKILL folder {directory} is skill folder but have more than 1 skill:\n {skills_list}"
        )
        return None
    # EN: There must be at least one because the caller already verified a match exists.
    # RU: Здесь должен быть хотя бы один файл, т.к. вызывающий код уже проверил совпадение.
    skill_file = skills[0]
    name = _get_skill_prefix(skill_file.name)
    return {"name": name, "path": str(skill_file)}


def _assert_flat_skill(directory: Path, files: list[str]) -> list[dict[str, str]]:
    """
    EN: Return all *.skill.md files located directly in `directory`.
    RU: Вернуть все файлы *.skill.md, находящиеся непосредственно в `directory`.
    """
    results: list[dict[str, str]] = []
    for filename in files:
        prefix = _get_skill_prefix(filename)
        if prefix is None:
            continue
        results.append({"name": prefix, "path": str(directory / filename)})
    return results


def search_skills(
    folder: str | Path = ".", case_sensitive: bool = False
) -> list[dict[str, str]]:
    """
    EN: Recursively scan `folder` for *.skill.md files.
        Distinguish skill directories from flat skill folders.
        Return a list of dicts with keys 'name' and 'path'.
    RU: Рекурсивно просканировать `folder` на наличие файлов *.skill.md.
        Различать skill-директории и flat skill папки.
        Вернуть список словарей с ключами 'name' и 'path'.
    """
    root = Path(folder).resolve()
    if not root.is_dir():
        raise ValueError(f"Not a directory: {root}")

    results: list[dict[str, str]] = []

    # EN: os.walk yields (directory_path, subdirectories, files).
    # RU: os.walk возвращает (путь_к_директории, поддиректории, файлы).
    for current_dir, subdirs, files in os.walk(root):
        current_path = Path(current_dir)

        # EN: Skip excluded directories to avoid unnecessary traversal.
        # RU: Пропустить исключённые директории, чтобы избежать лишнего обхода.
        subdirs[:] = [d for d in subdirs if d not in EXCLUDED_DIRS]

        # EN: Filter skill files present directly in the current directory.
        # RU: Отфильтровать skill-файлы, находящиеся непосредственно в текущей директории.
        skill_files = [f for f in files if f.endswith(".skill.md")]

        if not skill_files:
            continue

        if _is_skill_dir(current_path, skill_files, case_sensitive):
            # EN: This directory is a skill folder; validate and add the single skill.
            # RU: Эта директория является skill-папкой; провалидировать и добавить единственный skill.
            entry = _assert_skill_dir(current_path, case_sensitive)
            if entry is not None:
                results.append(entry)
            # EN: Do not recurse deeper — subdirectories belong to this skill dir.
            # RU: Не углубляться дальше — поддиректории относятся к этой skill-директории.
            subdirs.clear()
        else:
            # EN: This is a flat skill folder; add all local *.skill.md files.
            # RU: Это flat skill папка; добавить все локальные *.skill.md файлы.
            results.extend(_assert_flat_skill(current_path, skill_files))

    return results


def main() -> int:
    """
    EN: CLI entry point. Parse arguments, run the search, and log results.
    RU: Точка входа для CLI. Разобрать аргументы, запустить поиск и залогировать результаты.
    """
    logging.basicConfig(level=logging.INFO, format="%(levelname)s: %(message)s")

    parser = argparse.ArgumentParser(
        description="Search for *.skill.md files recursively."
    )
    parser.add_argument(
        "folder",
        nargs="?",
        default=".",
        help=(
            "EN: Folder to search (default: current directory).\n"
            "RU: Папка для поиска (по умолчанию: текущая директория)."
        ),
    )
    parser.add_argument(
        "--case",
        action="store_true",
        help=(
            "EN: Enable case-sensitive comparison between folder name and skill prefix.\n"
            "RU: Включить чувствительность к регистру при сравнении имени папки и префикса skill."
        ),
    )
    args = parser.parse_args()

    skills = search_skills(folder=args.folder, case_sensitive=args.case)

    # EN: When called directly, print the found skills as info logs.
    # RU: При прямом вызове вывести найденные skills в info лог.
    for skill in skills:
        logging.info(f"Skill found: {skill['name']} -> {skill['path']}")

    return 0


if __name__ == "__main__":
    sys.exit(main())
