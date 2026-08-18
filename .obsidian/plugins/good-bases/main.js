var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __export = (target, all) => {
  for (var name in all)
    __defProp(target, name, { get: all[name], enumerable: true });
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);

// src/main.ts
var main_exports = {};
__export(main_exports, {
  default: () => NotionBasesPlugin
});
module.exports = __toCommonJS(main_exports);
var import_obsidian5 = require("obsidian");

// src/constants.ts
var NOTION_TABLE_VIEW = "notion-table";
var LOG_PREFIX = "[good-bases]";

// src/view-options.ts
function buildViewOptions(_config) {
  return [
    {
      type: "toggle",
      key: "wrapCells",
      displayName: "Wrap all content",
      default: true
    },
    {
      type: "toggle",
      key: "verticalLines",
      displayName: "Show vertical lines",
      default: true
    },
    {
      type: "dropdown",
      key: "openMode",
      displayName: "Open notes in",
      default: "tab",
      options: {
        tab: "New tab",
        panel: "Page panel"
      }
    },
    {
      type: "multitext",
      key: "pillProperties",
      displayName: "Properties to show as colored pills"
    },
    {
      type: "multitext",
      key: "pinnedColors",
      displayName: "Pinned pill colors (value=color: gray, brown, orange, yellow, green, blue, purple, pink, red)"
    }
  ];
}

// src/view/notion-table-view.ts
var import_obsidian4 = require("obsidian");

// src/lib/colors.ts
var NOTION_COLORS = [
  { name: "gray", lightBg: "#EBECED", lightFg: "#9B9A97", darkBg: "#454B4E", darkFg: "rgba(151,154,155,0.95)" },
  { name: "brown", lightBg: "#E9E5E3", lightFg: "#64473A", darkBg: "#434040", darkFg: "#937264" },
  { name: "orange", lightBg: "#FAEBDD", lightFg: "#D9730D", darkBg: "#594A3A", darkFg: "#FFA344" },
  { name: "yellow", lightBg: "#FBF3DB", lightFg: "#DFAB01", darkBg: "#59563B", darkFg: "#FFDC49" },
  { name: "green", lightBg: "#DDEDEA", lightFg: "#0F7B6C", darkBg: "#354C4B", darkFg: "#4DAB9A" },
  { name: "blue", lightBg: "#DDEBF1", lightFg: "#0B6E99", darkBg: "#364954", darkFg: "#529CCA" },
  { name: "purple", lightBg: "#EAE4F2", lightFg: "#6940A5", darkBg: "#443F57", darkFg: "#9A6DD7" },
  { name: "pink", lightBg: "#F4DFEB", lightFg: "#AD1A72", darkBg: "#533B4C", darkFg: "#E255A1" },
  { name: "red", lightBg: "#FBE4E4", lightFg: "#E03E3E", darkBg: "#594141", darkFg: "#FF7369" }
];
function colorFor(text) {
  let h = 0;
  for (let i = 0; i < text.length; i++) {
    h = h * 31 + text.charCodeAt(i) >>> 0;
  }
  return NOTION_COLORS[h % NOTION_COLORS.length];
}
function colorByName(name) {
  return NOTION_COLORS.find((c) => c.name === name);
}
function colorKey(text) {
  return text.replace(/^#/, "").toLowerCase();
}
function resolvePillColor(text, pinned) {
  const key = colorKey(text);
  return pinned.get(key) ?? colorFor(key);
}
function applyColorVars(el, c) {
  el.setCssProps({
    "--ntn-pill-bg-light": c.lightBg,
    "--ntn-pill-fg-light": c.lightFg,
    "--ntn-pill-bg-dark": c.darkBg,
    "--ntn-pill-fg-dark": c.darkFg
  });
}
function applyPillColor(pill, text, pinned) {
  applyColorVars(pill, resolvePillColor(text, pinned));
}

// src/lib/pills.ts
var import_obsidian = require("obsidian");
function bareName(prop) {
  return prop.split(".").slice(1).join(".");
}
function computePillProps(props, entries, config, app) {
  const pillProps = /* @__PURE__ */ new Set();
  const listProps = /* @__PURE__ */ new Set();
  const userListed = config.get("pillProperties");
  const userSet = new Set(
    Array.isArray(userListed) ? userListed.map((s) => String(s).toLowerCase().trim()) : []
  );
  const mtm = app.metadataTypeManager;
  for (const prop of props) {
    const bare = bareName(prop).toLowerCase();
    const display = config.getDisplayName(prop).toLowerCase();
    const info = mtm?.getPropertyInfo?.(bare);
    const metaType = typeof info === "string" ? info : info?.widget ?? info?.type;
    let isList = metaType === "multitext" || metaType === "tags" || metaType === "aliases";
    if (!isList) {
      for (const entry of entries) {
        if (entry.getValue(prop) instanceof import_obsidian.ListValue) {
          isList = true;
          break;
        }
      }
    }
    if (isList) listProps.add(prop);
    if (isList || bare === "tags" || userSet.has(bare) || userSet.has(display)) {
      pillProps.add(prop);
    }
  }
  return { pillProps, listProps };
}
function parsePinnedColors(raw) {
  const pinned = /* @__PURE__ */ new Map();
  if (!Array.isArray(raw)) return pinned;
  for (const item of raw) {
    const m = String(item).match(/^(.+?)\s*[=:]\s*(.+)$/);
    if (!m) continue;
    const value = m[1].trim().replace(/^#/, "").toLowerCase();
    const colorName = m[2].trim().toLowerCase();
    const color = colorByName(colorName);
    if (value && color) pinned.set(value, color);
  }
  return pinned;
}

// src/lib/values.ts
var import_obsidian2 = require("obsidian");
function valueToStrings(value) {
  if (value === null) return [];
  if (value instanceof import_obsidian2.ListValue) {
    const out = [];
    for (let i = 0; i < value.length(); i++) {
      const s2 = value.get(i).toString();
      if (s2) out.push(s2);
    }
    return out;
  }
  const s = value.toString();
  return s ? [s] : [];
}

// src/view/note-modal.ts
var import_obsidian3 = require("obsidian");

// src/lib/frontmatter.ts
function splitFrontmatter(raw) {
  const match = raw.match(/^---\r?\n(?:[\s\S]*?\r?\n)?---(?:\r?\n|$)/);
  if (!match) return { frontmatter: "", body: raw };
  return { frontmatter: match[0], body: raw.slice(match[0].length) };
}

// src/view/note-modal.ts
var NotePageModal = class extends import_obsidian3.Modal {
  constructor(app, file, deps) {
    super(app);
    /** Lifetime owner for MarkdownRenderer children; replaced per render. */
    this.renderComp = null;
    this.saveTimer = null;
    /** True while the textarea holds keystrokes not yet written to disk. */
    this.dirty = false;
    this.file = file;
    this.deps = deps;
  }
  onOpen() {
    void this.buildPanel();
  }
  async buildPanel() {
    const { contentEl, modalEl } = this;
    modalEl.addClass("ntn-page-modal");
    contentEl.addClass("ntn-page-content");
    const openTabBtn = modalEl.createDiv({
      cls: "ntn-page-open-tab",
      attr: { "aria-label": "Open in new tab", tabindex: "0" }
    });
    (0, import_obsidian3.setIcon)(openTabBtn, "maximize-2");
    const openInTab = () => {
      this.close();
      void this.app.workspace.getLeaf(true).openFile(this.file);
    };
    openTabBtn.addEventListener("click", openInTab);
    openTabBtn.addEventListener("keydown", (evt) => {
      if (evt.key === "Enter" || evt.key === " ") {
        evt.preventDefault();
        openInTab();
      }
    });
    this.pageTitleEl = contentEl.createEl("h1", {
      cls: "ntn-page-title",
      // plaintext-only keeps pasted rich text from injecting HTML
      // (supported in Electron/Chromium, which is all Obsidian runs on).
      attr: { contenteditable: "plaintext-only", "data-placeholder": "Untitled" }
    });
    this.pageTitleEl.setText(this.file.basename);
    this.pageTitleEl.addEventListener("keydown", (evt) => {
      if (evt.key === "Enter") {
        evt.preventDefault();
        this.editBody();
      }
    });
    this.pageTitleEl.addEventListener("blur", () => void this.commitTitle());
    const { frontmatter, body } = splitFrontmatter(await this.app.vault.read(this.file));
    this.propsEl = contentEl.createDiv({ cls: "ntn-page-props" });
    this.renderPropertyRows(this.parseProperties(frontmatter));
    this.previewEl = contentEl.createDiv({
      cls: "ntn-page-preview markdown-rendered"
    });
    this.previewEl.addEventListener("click", (evt) => {
      const link = evt.target.closest("a");
      if (link?.hasClass("internal-link")) {
        evt.preventDefault();
        const target = link.getAttribute("data-href") ?? link.getAttribute("href") ?? "";
        this.close();
        void this.app.workspace.openLinkText(target, this.file.path, import_obsidian3.Keymap.isModEvent(evt));
        return;
      }
      if (link) return;
      this.editBody();
    });
    this.bodyArea = contentEl.createEl("textarea", {
      cls: "ntn-page-body ntn-hidden",
      attr: { placeholder: "Write something\u2026" }
    });
    this.bodyArea.value = body;
    this.bodyArea.addEventListener("input", () => {
      this.autoSizeBody();
      this.scheduleSave();
    });
    this.bodyArea.addEventListener("blur", () => {
      if (this.dirty) void this.saveBody();
      void this.showPreview();
    });
    await this.renderPreview();
    this.pageTitleEl.focus();
    contentEl.win.getSelection()?.selectAllChildren(this.pageTitleEl);
  }
  // ---------------- Body: preview <-> edit ----------------
  /** Swap the rendered body for the markdown textarea and focus it. */
  editBody() {
    this.previewEl.addClass("ntn-hidden");
    this.bodyArea.removeClass("ntn-hidden");
    this.autoSizeBody();
    this.bodyArea.focus();
  }
  /**
   * Grow the textarea to fit its content so the whole body stays readable
   * while editing — the panel scrolls, the textarea never does. Without
   * this, a long properties list plus the title can crush a flex-sized
   * textarea to nothing (many-properties bug, July 2026).
   */
  autoSizeBody() {
    this.bodyArea.setCssStyles({ height: "auto" });
    this.bodyArea.setCssStyles({ height: `${this.bodyArea.scrollHeight + 2}px` });
  }
  /** Swap the textarea back for the freshly rendered markdown. */
  async showPreview() {
    this.bodyArea.addClass("ntn-hidden");
    this.previewEl.removeClass("ntn-hidden");
    await this.renderPreview();
  }
  async renderPreview() {
    this.renderComp?.unload();
    this.renderComp = new import_obsidian3.Component();
    this.renderComp.load();
    this.previewEl.empty();
    const markdown = this.bodyArea.value;
    if (!markdown.trim()) {
      this.previewEl.createDiv({
        cls: "ntn-page-preview-empty",
        text: "Write something\u2026"
      });
      return;
    }
    await import_obsidian3.MarkdownRenderer.render(
      this.app,
      markdown,
      this.previewEl,
      this.file.path,
      this.renderComp
    );
  }
  // ---------------- Properties ----------------
  /** Re-read the file and rebuild the property rows from what's on disk. */
  async refreshProperties() {
    const { frontmatter } = splitFrontmatter(await this.app.vault.read(this.file));
    this.renderPropertyRows(this.parseProperties(frontmatter));
  }
  renderPropertyRows(props) {
    this.propsEl.empty();
    for (const [key, value] of Object.entries(props)) {
      this.renderPropertyRow(key, value);
    }
  }
  renderPropertyRow(key, value) {
    const row = this.propsEl.createDiv({ cls: "ntn-page-prop" });
    row.createDiv({ cls: "ntn-page-prop-name", text: key });
    const valueEl = row.createDiv({ cls: "ntn-page-prop-value" });
    const isPill = this.deps.isPillProp(key) || Array.isArray(value);
    const editable = key !== "tags";
    if (isPill) {
      const items = Array.isArray(value) ? value.map((v) => this.formatScalar(v)) : this.isEmpty(value) ? [] : [this.formatScalar(value)];
      for (const item of items) {
        const pill = valueEl.createSpan({ cls: "ntn-pill" });
        this.deps.applyColor(pill, item);
        pill.setText(item.replace(/^#/, ""));
      }
      if (!items.length) this.renderEmpty(valueEl);
      if (editable) {
        valueEl.addClass("ntn-page-prop-editable");
        valueEl.addEventListener("click", () => {
          this.deps.openSelect({
            anchor: valueEl,
            file: this.file,
            propName: key,
            current: items,
            isList: this.deps.isListProp(key) || Array.isArray(value),
            onWrite: () => void this.refreshProperties()
          });
        });
        this.deps.reanchorSelect(valueEl, this.file.path, key);
      }
      return;
    }
    if (typeof value === "boolean") {
      const cb = valueEl.createEl("input", { type: "checkbox", cls: "ntn-checkbox" });
      cb.checked = value;
      cb.addEventListener("change", () => void this.writeAndRefresh(key, cb.checked));
      return;
    }
    if (this.isEmpty(value)) this.renderEmpty(valueEl);
    else valueEl.createSpan({ text: this.formatScalar(value) });
    if (editable) {
      valueEl.addClass("ntn-page-prop-editable");
      valueEl.addEventListener("click", () => this.editScalar(valueEl, key, value));
    }
  }
  /** Swap a property row's value for an input; Enter/blur commits, Esc cancels. */
  editScalar(valueEl, key, value) {
    if (valueEl.querySelector(".ntn-input")) return;
    const kind = typeof value === "number" ? "number" : "text";
    const current = this.isEmpty(value) ? "" : this.formatScalar(value);
    valueEl.empty();
    const input = valueEl.createEl("input", {
      type: "text",
      cls: "ntn-input ntn-page-prop-input"
    });
    input.value = current;
    input.focus();
    input.select();
    let committed = false;
    const commit = () => {
      if (committed) return;
      committed = true;
      const raw = input.value.trim();
      if (raw === current) {
        void this.refreshProperties();
        return;
      }
      let out = raw;
      if (kind === "number") {
        const n = Number(raw);
        out = raw === "" ? null : Number.isNaN(n) ? raw : n;
      } else if (raw === "") {
        out = null;
      }
      void this.writeAndRefresh(key, out);
    };
    input.addEventListener("keydown", (evt) => {
      if (evt.key === "Enter") {
        evt.preventDefault();
        commit();
      } else if (evt.key === "Escape") {
        committed = true;
        evt.preventDefault();
        evt.stopPropagation();
        void this.refreshProperties();
      }
    });
    input.addEventListener("blur", commit);
  }
  async writeAndRefresh(propName, value) {
    await this.deps.write(this.file, propName, value);
    await this.refreshProperties();
  }
  parseProperties(frontmatter) {
    if (!frontmatter) return {};
    const yaml = frontmatter.replace(/^---\r?\n/, "").replace(/---\r?\n?$/, "");
    try {
      const parsed = (0, import_obsidian3.parseYaml)(yaml);
      if (parsed && typeof parsed === "object" && !Array.isArray(parsed)) {
        return parsed;
      }
    } catch (e) {
      console.error(`${LOG_PREFIX} failed to parse note frontmatter`, e);
    }
    return {};
  }
  isEmpty(value) {
    return value === null || value === void 0 || value === "";
  }
  renderEmpty(valueEl) {
    valueEl.createSpan({ cls: "ntn-page-prop-empty", text: "Empty" });
  }
  /** YAML scalars as Notion would show them (dates as yyyy-mm-dd). */
  formatScalar(value) {
    if (value instanceof Date) return value.toISOString().slice(0, 10);
    return String(value);
  }
  // ---------------- Body ----------------
  /** Debounced autosave: write 400ms after the last keystroke. */
  scheduleSave() {
    this.dirty = true;
    const win = this.contentEl.win;
    if (this.saveTimer !== null) win.clearTimeout(this.saveTimer);
    this.saveTimer = win.setTimeout(() => void this.saveBody(), 400);
  }
  async saveBody() {
    this.dirty = false;
    try {
      await this.app.vault.process(
        this.file,
        (data) => splitFrontmatter(data).frontmatter + this.bodyArea.value
      );
    } catch (e) {
      console.error(`${LOG_PREFIX} failed to save note body`, e);
      new import_obsidian3.Notice("Couldn't save the note.");
    }
  }
  // ---------------- Title ----------------
  /** Rename the file to match the title; revert the h1 if that fails. */
  async commitTitle() {
    const title = (this.pageTitleEl.textContent ?? "").replace(/\s+/g, " ").trim();
    if (!title || title === this.file.basename) return;
    const folder = this.file.parent?.path ?? "";
    const newPath = folder && folder !== "/" ? `${folder}/${title}.md` : `${title}.md`;
    try {
      await this.app.fileManager.renameFile(this.file, newPath);
    } catch (e) {
      console.error(`${LOG_PREFIX} failed to rename note`, e);
      new import_obsidian3.Notice(`Couldn't rename the note to "${title}".`);
      this.pageTitleEl.setText(this.file.basename);
    }
  }
  onClose() {
    this.deps.closeSelect();
    this.renderComp?.unload();
    this.renderComp = null;
    if (this.saveTimer !== null) this.contentEl.win.clearTimeout(this.saveTimer);
    void this.flushPendingEdits();
    this.contentEl.empty();
  }
  async flushPendingEdits() {
    if (this.dirty) await this.saveBody();
    await this.commitTitle();
  }
};

// src/view/select-editor.ts
var SelectEditor = class {
  constructor(deps) {
    this.deps = deps;
    /** Open color-picker flyout, if any (a sibling popover on the body). */
    this.colorMenu = null;
    this.closed = false;
    /** Distinct known values for this property: lowercase key → display text. */
    this.known = /* @__PURE__ */ new Map();
    const { entries, current, prop } = deps;
    for (const e of entries) {
      for (const s of valueToStrings(e.getValue(prop))) {
        const display = s.replace(/^#/, "");
        if (display && !this.known.has(display.toLowerCase())) {
          this.known.set(display.toLowerCase(), display);
        }
      }
    }
    this.selected = current.map((s) => s.replace(/^#/, ""));
    this.menu = this.build();
    this.position();
  }
  /** Whether the given node lives inside the menu or its color flyout. */
  contains(node) {
    if (!node) return false;
    return this.menu.contains(node) || (this.colorMenu?.contains(node) ?? false);
  }
  /** The cell element this menu is anchored to (drives click-to-toggle). */
  get anchorEl() {
    return this.deps.anchor;
  }
  /**
   * Re-point the menu at a freshly rendered cell for the same file +
   * property. `onDataUpdated` replaces every `td`, so without this the
   * anchor would dangle on a detached node and click-to-toggle (which
   * compares against the live cell) would miss. Returns whether it matched.
   */
  reanchorIfMatches(td, filePath, prop) {
    if (this.closed) return false;
    if (this.deps.prop !== prop || this.deps.file.path !== filePath) return false;
    this.deps.anchor = td;
    return true;
  }
  /** Tear the menu down. Idempotent; notifies the owner via `onClose`. */
  close() {
    if (this.closed) return;
    this.closed = true;
    this.closeColorMenu();
    this.menu.remove();
    this.deps.onClose();
  }
  closeColorMenu() {
    this.colorMenu?.remove();
    this.colorMenu = null;
  }
  build() {
    const { doc, isList } = this.deps;
    const menu = doc.body.createDiv({ cls: "ntn-root ntn-select-menu" });
    const currentEl = menu.createDiv({ cls: "ntn-select-current" });
    this.pillsWrap = currentEl.createDiv({ cls: "ntn-select-pills" });
    this.input = currentEl.createEl("input", {
      type: "text",
      cls: "ntn-select-input",
      attr: { placeholder: "Search or create\u2026", spellcheck: "false" }
    });
    menu.createDiv({
      cls: "ntn-select-hint",
      text: isList ? "Select options or create one" : "Select an option or create one"
    });
    this.optionsEl = menu.createDiv({ cls: "ntn-select-options" });
    this.input.addEventListener("input", () => this.renderOptions());
    this.input.addEventListener("keydown", (evt) => this.onKeydown(evt));
    this.renderPills();
    this.renderOptions();
    this.input.focus();
    return menu;
  }
  /** Empty selection deletes the property (`write` treats `null` as delete). */
  write() {
    const out = this.deps.isList ? this.selected.length ? this.selected : null : this.selected[0] ?? null;
    this.deps.write(out);
  }
  renderPills() {
    this.pillsWrap.empty();
    for (const v of this.selected) {
      const pill = this.pillsWrap.createSpan({ cls: "ntn-pill" });
      this.deps.applyColor(pill, v);
      pill.createSpan({ text: v });
      const x = pill.createSpan({ cls: "ntn-pill-remove", text: "\u2715" });
      x.addEventListener("click", (evt) => {
        evt.stopPropagation();
        this.selected = this.selected.filter((s) => s !== v);
        this.write();
        this.renderPills();
        this.renderOptions();
      });
    }
  }
  pick(v) {
    if (this.deps.isList) {
      const has = this.selected.some((s) => s.toLowerCase() === v.toLowerCase());
      this.selected = has ? this.selected.filter((s) => s.toLowerCase() !== v.toLowerCase()) : [...this.selected, v];
      if (!this.known.has(v.toLowerCase())) this.known.set(v.toLowerCase(), v);
      this.write();
      this.input.value = "";
      this.renderPills();
      this.renderOptions();
      this.input.focus();
    } else {
      this.selected = [v];
      this.write();
      this.close();
    }
  }
  renderOptions() {
    this.closeColorMenu();
    this.optionsEl.empty();
    const q = this.input.value.trim();
    const ql = q.toLowerCase();
    const visible = [...this.known.values()].filter((o) => !ql || o.toLowerCase().includes(ql)).sort((a, b) => a.localeCompare(b, void 0, { sensitivity: "base" }));
    for (const o of visible) {
      const row = this.optionsEl.createDiv({ cls: "ntn-select-option" });
      const colorBtn = row.createSpan({
        cls: "ntn-select-color-btn",
        attr: { "aria-label": "Change color" }
      });
      this.deps.applyColor(colorBtn, o);
      colorBtn.addEventListener("click", (evt) => {
        evt.stopPropagation();
        this.openColorMenu(colorBtn, o);
      });
      const pill = row.createSpan({ cls: "ntn-pill" });
      this.deps.applyColor(pill, o);
      pill.setText(o);
      if (this.selected.some((s) => s.toLowerCase() === o.toLowerCase())) {
        row.createSpan({ cls: "ntn-select-check", text: "\u2713" });
      }
      row.addEventListener("click", () => this.pick(o));
    }
    if (q && !this.known.has(ql)) {
      const row = this.optionsEl.createDiv({ cls: "ntn-select-option" });
      row.createSpan({ cls: "ntn-select-create", text: "Create" });
      const pill = row.createSpan({ cls: "ntn-pill" });
      this.deps.applyColor(pill, q);
      pill.setText(q);
      row.addEventListener("click", () => this.pick(q));
    }
    if (!visible.length && !q) {
      this.optionsEl.createDiv({
        cls: "ntn-select-empty",
        text: "No options yet \u2014 type to create one"
      });
    }
  }
  onKeydown(evt) {
    if (evt.key === "Escape") {
      evt.preventDefault();
      evt.stopPropagation();
      this.close();
    } else if (evt.key === "Enter") {
      const q = this.input.value.trim();
      if (q) this.pick(this.known.get(q.toLowerCase()) ?? q);
      else this.close();
    } else if (evt.key === "Backspace" && this.input.value === "" && this.deps.isList && this.selected.length) {
      this.selected = this.selected.slice(0, -1);
      this.write();
      this.renderPills();
      this.renderOptions();
    }
  }
  /** Open the color picker for a value, anchored to its row button. */
  openColorMenu(anchorEl, value) {
    this.closeColorMenu();
    const menu = this.deps.doc.body.createDiv({ cls: "ntn-root ntn-color-menu" });
    this.colorMenu = menu;
    for (const c of NOTION_COLORS) {
      const item = menu.createDiv({ cls: "ntn-color-option" });
      const swatch = item.createSpan({ cls: "ntn-color-swatch" });
      applyColorVars(swatch, c);
      item.createSpan({ cls: "ntn-color-name", text: c.name });
      item.addEventListener("click", (evt) => {
        evt.stopPropagation();
        this.deps.setColor(value, c.name);
        this.renderPills();
        this.renderOptions();
      });
    }
    this.clampToWindow(menu, anchorEl.getBoundingClientRect());
  }
  /** Anchor the main menu below the cell, then clamp into the window. */
  position() {
    const rect = this.deps.anchor.getBoundingClientRect();
    this.menu.setCssStyles({ minWidth: `${Math.max(rect.width, 220)}px` });
    this.clampToWindow(this.menu, rect);
  }
  /** Place a popover just below `anchorRect`, nudged to stay on screen. */
  clampToWindow(el, anchorRect) {
    const { win } = this.deps;
    el.setCssStyles({
      left: `${anchorRect.left}px`,
      top: `${anchorRect.bottom + 4}px`
    });
    const rect = el.getBoundingClientRect();
    if (rect.bottom > win.innerHeight - 8) {
      el.setCssStyles({ top: `${Math.max(8, anchorRect.top - rect.height - 4)}px` });
    }
    if (rect.right > win.innerWidth - 8) {
      el.setCssStyles({ left: `${Math.max(8, win.innerWidth - rect.width - 8)}px` });
    }
  }
};

// src/view/notion-table-view.ts
var NotionTableView = class extends import_obsidian4.BasesView {
  constructor(controller, parentEl) {
    super(controller);
    this.type = NOTION_TABLE_VIEW;
    /** True while the toolbar's New button is rerouted to the page panel. */
    this.newButtonPatched = false;
    /** Pill / list classification, recomputed each update. */
    this.pills = { pillProps: /* @__PURE__ */ new Set(), listProps: /* @__PURE__ */ new Set() };
    /** User-pinned value → color overrides from the `pinnedColors` view option. */
    this.pinnedColors = /* @__PURE__ */ new Map();
    /** The open select editor, if any (also drives outside-click detection). */
    this.selectEditor = null;
    this.queryCtrl = controller;
    this.rootEl = parentEl.createDiv({ cls: "ntn-root" });
    this.register(() => this.closeSelectMenu());
    this.registerDomEvent(this.rootEl.doc, "mousedown", (evt) => {
      if (!this.selectEditor) return;
      const target = evt.target;
      if (this.selectEditor.contains(target)) return;
      if (this.selectEditor.anchorEl.contains(target)) return;
      this.closeSelectMenu();
    }, { capture: true });
    this.patchToolbarNew();
  }
  onDataUpdated() {
    this.patchToolbarNew();
    const root = this.rootEl;
    root.empty();
    root.toggleClass("ntn-wrap", this.config.get("wrapCells") !== false);
    root.toggleClass("ntn-vlines", this.config.get("verticalLines") !== false);
    const props = this.config.getOrder();
    this.pills = computePillProps(props, this.data.data, this.config, this.app);
    this.pinnedColors = parsePinnedColors(this.config.get("pinnedColors"));
    const table = root.createEl("table", { cls: "ntn-table" });
    const thead = table.createEl("thead");
    const headRow = thead.createEl("tr");
    const thTitle = headRow.createEl("th", { cls: "ntn-th ntn-col-title" });
    thTitle.createSpan({ cls: "ntn-th-icon", text: "Aa" });
    thTitle.createSpan({ text: "Name" });
    for (const prop of props) {
      const th = headRow.createEl("th", { cls: "ntn-th" });
      th.createSpan({ text: this.config.getDisplayName(prop) });
    }
    const tbody = table.createEl("tbody");
    const colCount = props.length + 1;
    for (const group of this.data.groupedData) {
      if (group.hasKey() && group.key) {
        const gRow = tbody.createEl("tr", { cls: "ntn-group-row" });
        const gCell = gRow.createEl("td", { attr: { colspan: String(colCount) } });
        const pill = gCell.createSpan({ cls: "ntn-pill" });
        this.applyPillColor(pill, group.key.toString());
        pill.setText(group.key.toString());
        gCell.createSpan({ cls: "ntn-group-count", text: String(group.entries.length) });
      }
      for (const entry of group.entries) {
        this.renderRow(tbody, entry, props);
      }
    }
    const newRow = root.createDiv({ cls: "ntn-new-row" });
    newRow.createSpan({ cls: "ntn-new-plus", text: "+" });
    newRow.createSpan({ text: "New" });
    newRow.addEventListener("click", () => void this.createAndOpenPage());
  }
  renderRow(tbody, entry, props) {
    const tr = tbody.createEl("tr", { cls: "ntn-row" });
    const titleTd = tr.createEl("td", { cls: "ntn-td ntn-col-title" });
    const titleWrap = titleTd.createDiv({ cls: "ntn-title-wrap" });
    titleWrap.createSpan({ cls: "ntn-page-icon", text: "\u{1F4C4}" });
    const link = titleWrap.createSpan({
      cls: "ntn-title-text",
      text: entry.file.basename
    });
    link.addEventListener("click", (evt) => {
      void this.app.workspace.openLinkText(
        entry.file.path,
        "",
        evt.ctrlKey || evt.metaKey
      );
    });
    const openBtn = titleWrap.createSpan({ cls: "ntn-open-btn", text: "OPEN" });
    openBtn.addEventListener("click", (evt) => {
      evt.stopPropagation();
      if (this.config.get("openMode") === "panel") {
        this.openPagePanel(entry.file);
      } else {
        void this.app.workspace.openLinkText(entry.file.path, "", true);
      }
    });
    for (const prop of props) {
      const td = tr.createEl("td", { cls: "ntn-td" });
      this.renderCell(td, entry, prop);
    }
  }
  renderCell(td, entry, prop) {
    const value = entry.getValue(prop);
    const editable = prop.startsWith("note.");
    const propName = prop.split(".").slice(1).join(".");
    if (this.pills.pillProps.has(prop)) {
      const wrap = td.createDiv({ cls: "ntn-pills" });
      const items = valueToStrings(value);
      for (const item of items) {
        const pill = wrap.createSpan({ cls: "ntn-pill" });
        this.applyPillColor(pill, item);
        pill.setText(item.replace(/^#/, ""));
      }
      if (editable && propName !== "tags") {
        td.addClass("ntn-editable");
        td.addEventListener(
          "click",
          () => this.openSelectEditor(td, entry, prop, propName)
        );
        this.selectEditor?.reanchorIfMatches(td, entry.file.path, prop);
      }
      return;
    }
    if (value instanceof import_obsidian4.BooleanValue) {
      const cb = td.createEl("input", { type: "checkbox", cls: "ntn-checkbox" });
      cb.checked = value.isTruthy();
      if (editable) {
        cb.addEventListener("change", () => {
          void this.writeProperty(entry.file, propName, cb.checked);
        });
      } else {
        cb.disabled = true;
      }
      return;
    }
    const cellEl = td.createDiv({ cls: "ntn-cell" });
    if (value !== null) {
      value.renderTo(cellEl, this.app.renderContext);
    }
    if (editable) {
      td.addClass("ntn-editable");
      const kind = value instanceof import_obsidian4.NumberValue ? "number" : "text";
      td.addEventListener("click", (evt) => {
        if (evt.target.closest("a")) return;
        this.editCell(td, entry, propName, value ? value.toString() : "", kind);
      });
    }
  }
  /** Swap a cell's content for an input; commit on Enter/blur, cancel on Esc. */
  editCell(td, entry, propName, current, kind) {
    if (td.querySelector(".ntn-input")) return;
    const rect = td.getBoundingClientRect();
    const multiline = kind === "text" && rect.height > 40;
    td.empty();
    const input = multiline ? td.createEl("textarea", { cls: "ntn-input ntn-textarea" }) : td.createEl("input", { type: "text", cls: "ntn-input" });
    input.setCssStyles({ width: `${rect.width}px`, height: `${rect.height}px` });
    input.value = current;
    input.focus();
    input.select();
    let committed = false;
    const commit = () => {
      if (committed) return;
      committed = true;
      const raw = input.value.trim();
      if (raw === current) {
        this.onDataUpdated();
        return;
      }
      let out = raw;
      if (kind === "number") {
        const n = Number(raw);
        out = raw === "" ? null : Number.isNaN(n) ? raw : n;
      } else if (raw === "") {
        out = null;
      }
      void this.writeProperty(entry.file, propName, out);
    };
    input.addEventListener("keydown", (ev) => {
      const evt = ev;
      if (evt.key === "Enter" && !evt.shiftKey) {
        evt.preventDefault();
        commit();
      } else if (evt.key === "Escape") {
        committed = true;
        this.onDataUpdated();
      }
    });
    input.addEventListener("blur", commit);
  }
  async writeProperty(file, propName, value) {
    try {
      await this.app.fileManager.processFrontMatter(file, (fm) => {
        if (value === null) {
          delete fm[propName];
        } else {
          fm[propName] = value;
        }
      });
    } catch (e) {
      console.error(`${LOG_PREFIX} failed to write property`, propName, e);
      new import_obsidian4.Notice(`Couldn't update "${propName}".`);
      this.onDataUpdated();
    }
  }
  /**
   * Reroute the core toolbar's New button to the page panel while this
   * view is active. The button lives on the query controller, outside this
   * view's DOM, so its menu's `open` is shadowed on the instance and
   * restored on unload. `newItemMenu` is internal API — if it ever moves,
   * the guard below simply leaves the core behavior untouched (and the
   * footer "+ New" falls back to its own capture flow).
   */
  patchToolbarNew() {
    if (this.newButtonPatched) return;
    const menu = this.queryCtrl.newItemMenu;
    if (!menu || typeof menu.open !== "function" || typeof menu.close !== "function") {
      return;
    }
    const orig = menu.open.bind(menu);
    const patched = async (name, fmProc) => {
      if (import_obsidian4.Platform.isPhone) return orig(name, fmProc);
      let created;
      const ref = this.app.vault.on("create", (file) => {
        if (file instanceof import_obsidian4.TFile) created = file;
      });
      const body = this.rootEl.doc.body;
      body.addClass("ntn-hide-new-popover");
      try {
        await orig(name, fmProc);
      } finally {
        this.app.vault.offref(ref);
        menu.close();
        body.removeClass("ntn-hide-new-popover");
      }
      if (created) this.openPagePanel(created);
    };
    menu.open = patched;
    this.newButtonPatched = true;
    this.register(() => {
      menu.open = orig;
      this.newButtonPatched = false;
    });
  }
  /**
   * "+ New" flow: create the note through the core Bases flow — so it lands
   * in the configured folder and gets the frontmatter implied by the view's
   * filters — then edit it in the centered Notion-style page panel instead
   * of the small popover Obsidian anchors to the toolbar's New button.
   */
  async createAndOpenPage() {
    if (import_obsidian4.Platform.isPhone || this.newButtonPatched) {
      await this.createFileForView();
      return;
    }
    let created;
    const ref = this.app.vault.on("create", (file) => {
      if (file instanceof import_obsidian4.TFile) created = file;
    });
    try {
      await this.createFileForView();
    } finally {
      this.app.vault.offref(ref);
    }
    if (!created) return;
    const doc = this.rootEl.doc;
    if (doc.querySelector(".bases-new-item-popover")) doc.body.click();
    this.openPagePanel(created);
  }
  /** Open a note centered in the Notion-style page panel. */
  openPagePanel(file) {
    new NotePageModal(this.app, file, {
      applyColor: (pill, text) => this.applyPillColor(pill, text),
      write: (f, propName, value) => this.writeProperty(f, propName, value),
      isPillProp: (name) => this.pills.pillProps.has(`note.${name}`),
      isListProp: (name) => this.pills.listProps.has(`note.${name}`),
      openSelect: (opts) => this.openSelectAt(opts),
      reanchorSelect: (anchor, filePath, propName) => void this.selectEditor?.reanchorIfMatches(
        anchor,
        filePath,
        `note.${propName}`
      ),
      closeSelect: () => this.closeSelectMenu()
    }).open();
  }
  /** Color a pill element using this view's pinned-color overrides. */
  applyPillColor(pill, text) {
    applyPillColor(pill, text, this.pinnedColors);
  }
  /** Open the Notion-style select editor for a pill cell of the table. */
  openSelectEditor(td, entry, prop, propName) {
    this.openSelectAt({
      anchor: td,
      file: entry.file,
      propName,
      current: valueToStrings(entry.getValue(prop)),
      isList: this.pills.listProps.has(prop)
    });
  }
  /**
   * Open the select editor anchored anywhere — a table cell or a property
   * row of the page panel. Known values always come from the live query
   * result; lifetime stays with the view (outside-click / Esc / unload).
   */
  openSelectAt(opts) {
    if (this.selectEditor?.anchorEl === opts.anchor) {
      this.closeSelectMenu();
      return;
    }
    this.closeSelectMenu();
    const prop = `note.${opts.propName}`;
    this.selectEditor = new SelectEditor({
      doc: this.rootEl.doc,
      win: this.rootEl.win,
      anchor: opts.anchor,
      entries: this.data.data,
      file: opts.file,
      current: opts.current,
      prop,
      isList: opts.isList,
      applyColor: (pill, text) => this.applyPillColor(pill, text),
      write: (value) => void this.writeProperty(opts.file, opts.propName, value).then(() => opts.onWrite?.()),
      setColor: (value, colorName) => this.setPinnedColor(value, colorName),
      onClose: () => {
        this.selectEditor = null;
      }
    });
  }
  /**
   * Pin a value to a specific Notion color. Updates the live map for instant
   * feedback in the open editor, then persists into the `pinnedColors` view
   * option (replacing any prior entry for the same value) so it survives
   * reloads and is editable from the view settings too.
   */
  setPinnedColor(value, colorName) {
    const color = colorByName(colorName);
    if (!color) return;
    const bare = value.replace(/^#/, "");
    const key = bare.toLowerCase();
    this.pinnedColors.set(key, color);
    const raw = this.config.get("pinnedColors");
    const list = Array.isArray(raw) ? raw.map((s) => String(s)) : [];
    const kept = list.filter((item) => {
      const m = item.match(/^(.+?)\s*[=:]\s*(.+)$/);
      return m ? m[1].trim().replace(/^#/, "").toLowerCase() !== key : true;
    });
    kept.push(`${bare}=${colorName}`);
    this.config.set("pinnedColors", kept);
  }
  closeSelectMenu() {
    this.selectEditor?.close();
    this.selectEditor = null;
  }
};

// src/main.ts
var NotionBasesPlugin = class extends import_obsidian5.Plugin {
  async onload() {
    if (typeof this.registerBasesView !== "function") {
      new import_obsidian5.Notice("GoodBases: requires Obsidian 1.10.0+ (registerBasesView API missing).", 8e3);
      return;
    }
    const ok = this.registerBasesView(NOTION_TABLE_VIEW, {
      name: "Notion-style table",
      icon: "lucide-table-2",
      factory: (controller, containerEl) => new NotionTableView(controller, containerEl),
      options: buildViewOptions
    });
    if (!ok) {
      new import_obsidian5.Notice("GoodBases: view registration failed. Is the Bases core plugin enabled?", 8e3);
    }
  }
};

/* nosourcemap */