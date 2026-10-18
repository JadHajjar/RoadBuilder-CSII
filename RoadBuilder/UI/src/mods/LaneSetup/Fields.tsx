import classNames from "classnames";
import { useLocalization } from "cs2/l10n";
import { Button, Scrollable, Tooltip } from "cs2/ui";
import { PropsWithChildren, ReactNode, useEffect, useState } from "react";
import { TextInputTheme } from "mods/Components/TextInput/TextInput";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { pickLaneSetupImage } from "./laneSetupBindings";
import styles from "./LaneSetup.module.scss";

export const useSetupTranslate = () => {
  const { translate } = useLocalization();
  return (key: string, fallback: string) => translate(`RoadBuilder.Setup.${key}`, fallback) ?? fallback;
};

export const Card = (
  props: PropsWithChildren<{ title: string; description?: string; collapsible?: boolean; defaultOpen?: boolean; right?: ReactNode }>
) => {
  const [open, setOpen] = useState(props.defaultOpen ?? true);
  const collapsed = props.collapsible && !open;

  return (
    <div className={styles.card}>
      <div className={classNames(styles.cardHeader, props.collapsible && styles.clickable)} onClick={props.collapsible ? () => setOpen(!open) : undefined}>
        {props.collapsible && <img className={classNames(styles.caret, open && styles.open)} style={{ maskImage: "url(Media/Glyphs/ThickStrokeArrowRight.svg)" }} />}
        <span className={styles.cardTitle}>{props.title}</span>
        {props.right && (
          <div className={styles.cardRight} onClick={(e) => e.stopPropagation()}>
            {props.right}
          </div>
        )}
      </div>
      {!collapsed && (
        <div className={styles.cardBody}>
          {props.description && <p className={styles.description}>{props.description}</p>}
          {props.children}
        </div>
      )}
    </div>
  );
};

export const Row = (props: PropsWithChildren<{ label: string; hint?: string }>) => (
  <div className={styles.row}>
    <div className={styles.rowLabel}>
      <span>{props.label}</span>
      {props.hint && <span className={styles.hint}>{props.hint}</span>}
    </div>
    <div className={styles.rowContent}>{props.children}</div>
  </div>
);

// Commits on blur or enter, so a change isn't applied to the prefab on every keystroke
// live commits on every keystroke, for fields that only hold local state and drive validation as you type
export const TextField = (props: {
  value?: string;
  placeholder?: string;
  disabled?: boolean;
  live?: boolean;
  onCommit: (value: string) => void;
  className?: string;
}) => {
  const [text, setText] = useState(props.value ?? "");

  useEffect(() => setText(props.value ?? ""), [props.value]);

  // Read-only fields never commit, even if the input still receives focus or keys
  const commit = () => {
    if (!props.disabled && text !== (props.value ?? "")) props.onCommit(text);
  };

  return (
    <input
      type="text"
      value={text}
      placeholder={props.placeholder}
      disabled={props.disabled}
      readOnly={props.disabled}
      className={classNames(TextInputTheme.input, styles.textField, props.className)}
      onChange={({ target }) => {
        if (props.disabled) return;

        setText(target.value);

        if (props.live) props.onCommit(target.value);
      }}
      onBlur={commit}
      onKeyDown={(e) => {
        if (e.key === "Enter") commit();
      }}
    />
  );
};

export const ImageField = (props: { value?: string; field: string; index?: number; disabled?: boolean; onCommit: (value: string) => void; onRemove?: () => void }) => {
  const t = useSetupTranslate();

  return (
    <div className={styles.imageField}>
      <div className={styles.imagePreview}>{props.value ? <img src={props.value} /> : <span>—</span>}</div>
      <TextField value={props.value} placeholder={t("ImageUrl", "Image URL")} disabled={props.disabled} onCommit={props.onCommit} />
      <Button variant="flat" className={styles.smallButton} disabled={props.disabled} onSelect={props.disabled ? undefined : () => pickLaneSetupImage(props.field, props.index ?? -1)}>
        {t("Browse", "Browse")}
      </Button>
      {(props.value || props.onRemove) && (
        <IconButton
          danger
          icon="Media/Glyphs/Close.svg"
          disabled={props.disabled}
          tooltip={t("Remove", "Remove")}
          onSelect={props.onRemove ?? (() => props.onCommit(""))}
        />
      )}
    </div>
  );
};

// A list of images, like the pieces a lane adds to the generated road thumbnail
export const ImageListField = (props: { values: string[]; field: string; disabled?: boolean; onCommit: (values: string[]) => void }) => {
  const t = useSetupTranslate();

  return (
    <div className={styles.imageList}>
      {props.values.length > 0 && (
        <div className={styles.imageStrip}>
          {props.values.map((x, i) => (
            <img key={i} src={x} />
          ))}
        </div>
      )}
      {props.values.map((value, index) => (
        <ImageField
          key={index}
          value={value}
          field={props.field}
          index={index}
          disabled={props.disabled}
          onCommit={(v) => props.onCommit(props.values.map((x, i) => (i === index ? v : x)))}
          onRemove={() => props.onCommit(props.values.filter((_, i) => i !== index))}
        />
      ))}
      <div className={styles.inlineButtons}>
        <Button variant="flat" className={styles.smallButton} disabled={props.disabled} onSelect={props.disabled ? undefined : () => pickLaneSetupImage(props.field, props.values.length)}>
          {t("AddImage", "Add image")}
        </Button>
        <TextField
          placeholder={t("AddImageUrl", "…or paste a URL and press Enter")}
          disabled={props.disabled}
          onCommit={(v) => v.trim() && props.onCommit([...props.values, v.trim()])}
        />
      </div>
    </div>
  );
};

export const Toggle = (props: { checked: boolean; label: string; hint?: string; disabled?: boolean; onChange: (value: boolean) => void }) => (
  <div className={classNames(styles.toggle, props.disabled && styles.disabled)} onClick={props.disabled ? undefined : () => props.onChange(!props.checked)}>
    <div className={classNames(styles.checkbox, props.checked && styles.checked)}>
      {props.checked && <img style={{ maskImage: "url(coui://roadbuildericons/RB_Checkmark.svg)" }} />}
    </div>
    <div className={styles.toggleText}>
      <span>{props.label}</span>
      {props.hint && <span className={styles.hint}>{props.hint}</span>}
    </div>
  </div>
);

export interface ChipItem<T> {
  value: T;
  label: string;
  icon?: string;
  tooltip?: string;
}

export const Chips = <T,>(props: { items: ChipItem<T>[]; isSelected: (value: T) => boolean; onSelect: (value: T) => void; disabled?: boolean }) => (
  <div className={styles.chips}>
    {props.items.map((item, i) => (
      <div
        key={i}
        title={item.tooltip}
        className={classNames(styles.chip, props.isSelected(item.value) && styles.selected, props.disabled && styles.disabled)}
        onClick={props.disabled ? undefined : () => props.onSelect(item.value)}
      >
        {item.icon && <img src={item.icon} />}
        <span>{item.label}</span>
      </div>
    ))}
  </div>
);

// Styled like the panel's other small buttons, the game's tool button theme draws a dark square that doesn't fit next to them
export const IconButton = (props: { icon: string; tooltip?: string; disabled?: boolean; danger?: boolean; onSelect: () => void }) => {
  const button = (
    <Button
      variant="flat"
      disabled={props.disabled}
      onSelect={props.disabled ? undefined : props.onSelect}
      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
      className={classNames(styles.iconButton, props.danger && styles.danger)}
    >
      <img style={{ maskImage: `url(${props.icon})` }} />
    </Button>
  );

  return props.tooltip && !props.disabled ? <Tooltip tooltip={props.tooltip}>{button}</Tooltip> : button;
};

export interface PickerItem {
  value: string;
  label: string;
  detail?: string;
}

// Searchable list, used for the many net sections and lane groups a lane can reference
export const Picker = (props: { value?: string; items: PickerItem[]; placeholder: string; disabled?: boolean; allowClear?: boolean; onSelect: (value: string) => void }) => {
  const t = useSetupTranslate();
  const [open, setOpen] = useState(false);
  const [filter, setFilter] = useState("");
  const current = props.items.find((x) => x.value === props.value);
  const query = filter.trim().toLowerCase();
  const filtered = query
    ? props.items.filter((x) => x.label.toLowerCase().includes(query) || x.value.toLowerCase().includes(query)).slice(0, 200)
    : props.items.slice(0, 200);

  const select = (value: string) => {
    setOpen(false);
    setFilter("");
    props.onSelect(value);
  };

  return (
    <div className={styles.picker}>
      <div className={classNames(styles.pickerValue, props.disabled && styles.disabled)} onClick={props.disabled ? undefined : () => setOpen(!open)}>
        <span className={classNames(!current && !props.value && styles.placeholder)}>{current?.label ?? props.value ?? props.placeholder}</span>
        <img className={classNames(styles.caret, open && styles.down)} style={{ maskImage: "url(Media/Glyphs/ThickStrokeArrowRight.svg)" }} />
      </div>
      {open && (
        <div className={styles.pickerPopup}>
          <input
            type="text"
            value={filter}
            placeholder={t("Search", "Search…")}
            className={classNames(TextInputTheme.input, styles.textField)}
            onChange={({ target }) => setFilter(target.value)}
          />
          <Scrollable className={styles.pickerList} vertical trackVisibility="scrollable">
            {props.allowClear && props.value && (
              <div className={styles.pickerItem} onClick={() => select("")}>
                <span className={styles.placeholder}>{t("Clear", "Clear")}</span>
              </div>
            )}
            {filtered.map((item) => (
              <div key={item.value} className={classNames(styles.pickerItem, item.value === props.value && styles.selected)} onClick={() => select(item.value)}>
                <span>{item.label}</span>
                {item.detail && <span className={styles.hint}>{item.detail}</span>}
              </div>
            ))}
            {filtered.length === 0 && <div className={styles.empty}>{t("NoMatches", "No matches")}</div>}
          </Scrollable>
        </div>
      )}
    </div>
  );
};
