import classNames from "classnames";
import { useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { Button } from "cs2/ui";
import { useState } from "react";
import { GetCategoryIcon, GetCategoryName, RoadCategory } from "domain/RoadCategory";
import {
  LaneOptionType,
  LaneSetupAggregateSection,
  LaneSetupCombination,
  LaneSetupDecoration,
  LaneSetupLaneInfo,
  LaneSetupOption,
  LaneSetupState,
  valueOptionTypes,
} from "domain/LaneSetup";
import { Chips, IconButton, ImageField, ImageListField, Picker, PickerItem, Row, TextField, Toggle, useSetupTranslate } from "./Fields";
import {
  addLaneSetupAggregateSection,
  createLaneSetupFromSection,
  addLaneSetupOption,
  addLaneSetupOptionValue,
  laneSetupGroups$,
  laneSetupPieceRequirements$,
  laneSetupSections$,
  moveLaneSetupOption,
  removeLaneSetupAggregateSection,
  removeLaneSetupOption,
  removeLaneSetupOptionValue,
  selectLaneSetupPrefab,
  setLaneSetupAggregateSection,
  setLaneSetupCombination,
  setLaneSetupList,
  setLaneSetupNumber,
  setLaneSetupOption,
  setLaneSetupOptionValue,
  setLaneSetupRequirements,
  setLaneSetupText,
} from "./laneSetupBindings";
import styles from "./LaneSetup.module.scss";

// Road types offered when picking where a lane can be used, Road (0) stands for plain asphalt roads
const categories = [
  RoadCategory.Road,
  RoadCategory.Highway,
  RoadCategory.PublicTransport,
  RoadCategory.Gravel,
  RoadCategory.Tiled,
  RoadCategory.Tram,
  RoadCategory.Train,
  RoadCategory.Subway,
  RoadCategory.Pathway,
  RoadCategory.Fence,
];
const nonRoadFlags = categories.filter((x) => x !== RoadCategory.Road);

// Same rule as NetworkConfigExtensionsUtil.MatchCategories
const allowsCategory = (info: LaneSetupLaneInfo, category: number) =>
  (category & info.RequireAll) === info.RequireAll && ((category & info.RequireAny) !== 0 || info.RequireAny === 0) && (category & info.RequireNone) === 0;

// Simple mode can only express "allowed in any of these road types", anything else is edited in advanced mode
const isSimpleCategorySetup = (info: LaneSetupLaneInfo) => info.RequireAll === 0 && (info.RequireAny === 0 || info.RequireNone === 0);

const writeAllowedCategories = (allowed: RoadCategory[]) => {
  // Plain roads have no category flag, so "allowed on plain roads" can only be written as excluding the other types
  if (allowed.includes(RoadCategory.Road)) {
    setLaneSetupNumber("LaneInfo.RequireAll", 0);
    setLaneSetupNumber("LaneInfo.RequireAny", 0);
    setLaneSetupNumber("LaneInfo.RequireNone", nonRoadFlags.filter((x) => !allowed.includes(x)).reduce((a, b) => a | b, 0));
  } else {
    setLaneSetupNumber("LaneInfo.RequireAll", 0);
    setLaneSetupNumber("LaneInfo.RequireAny", allowed.reduce((a, b) => a | b, 0));
    setLaneSetupNumber("LaneInfo.RequireNone", 0);
  }
};

export const CategoryEditor = (props: { info: LaneSetupLaneInfo; disabled: boolean }) => {
  const t = useSetupTranslate();
  const { translate } = useLocalization();
  const simple = isSimpleCategorySetup(props.info);
  const [advanced, setAdvanced] = useState(!simple);
  const items = categories.map((x) => ({
    value: x,
    label: x === RoadCategory.Road ? t("PlainRoads", "Roads") : translate(GetCategoryName(x), RoadCategory[x]) ?? RoadCategory[x],
    icon: GetCategoryIcon(x) ?? "Media/Game/Icons/Roads.svg",
  }));
  const allowed = categories.filter((x) => allowsCategory(props.info, x));

  const flagEditor = (field: "RequireAll" | "RequireAny" | "RequireNone", label: string, hint: string) => (
    <Row label={label} hint={hint}>
      <Chips
        items={items.filter((x) => x.value !== RoadCategory.Road)}
        disabled={props.disabled}
        isSelected={(x) => (props.info[field] & x) !== 0}
        onSelect={(x) => setLaneSetupNumber(`LaneInfo.${field}`, props.info[field] ^ x)}
      />
    </Row>
  );

  return (
    <>
      {!advanced ? (
        <Row label={t("AllowedIn", "Allowed in")} hint={t("AllowedInHint", "The road types this lane can be added to.")}>
          <Chips
            items={items}
            disabled={props.disabled}
            isSelected={(x) => allowed.includes(x)}
            onSelect={(x) => {
              const next = allowed.includes(x) ? allowed.filter((a) => a !== x) : [...allowed, x];
              if (next.length > 0) writeAllowedCategories(next);
            }}
          />
        </Row>
      ) : (
        <>
          {flagEditor("RequireAll", t("RequireAll", "Requires all of"), t("RequireAllHint", "Every selected type must be part of the road."))}
          {flagEditor("RequireAny", t("RequireAny", "Requires one of"), t("RequireAnyHint", "At least one selected type must be part of the road. Leave empty to allow plain roads."))}
          {flagEditor("RequireNone", t("RequireNone", "Not allowed in"), t("RequireNoneHint", "Roads with any selected type can't use this lane."))}
          <div className={styles.summary}>
            {t("CurrentlyAllowed", "Currently allowed in:")} {allowed.length ? allowed.map((x) => items.find((i) => i.value === x)?.label).join(", ") : t("Nothing", "nothing")}
          </div>
        </>
      )}
      <Toggle
        checked={advanced}
        label={t("AdvancedCategories", "Advanced category rules")}
        disabled={!simple && advanced}
        hint={!simple ? t("AdvancedCategoriesLocked", "The current rules can only be shown in advanced mode.") : undefined}
        onChange={setAdvanced}
      />
    </>
  );
};

// Edits a NetPieceRequirements[] field, which limits a section to edges or nodes in a given state
export const RequirementsEditor = (props: { field: string; label: string; hint?: string; values: number[]; disabled: boolean }) => {
  const t = useSetupTranslate();
  const all = useValue(laneSetupPieceRequirements$);
  const items: PickerItem[] = all.filter((x) => !props.values.includes(x.Value)).map((x) => ({ value: x.Value.toString(), label: x.Name }));

  return (
    <Row label={props.label} hint={props.hint}>
      <div className={styles.tags}>
        {props.values.map((value) => (
          <div key={value} className={styles.tag}>
            <span>{all.find((x) => x.Value === value)?.Name ?? value}</span>
            {!props.disabled && (
              <img
                style={{ maskImage: "url(Media/Glyphs/Close.svg)" }}
                onClick={() =>
                  setLaneSetupRequirements(
                    props.field,
                    props.values.filter((x) => x !== value)
                  )
                }
              />
            )}
          </div>
        ))}
      </div>
      <Picker
        items={items}
        placeholder={t("AddRequirement", "Add a requirement…")}
        disabled={props.disabled}
        onSelect={(value) => value && setLaneSetupRequirements(props.field, [...props.values, parseInt(value)])}
      />
    </Row>
  );
};

export const PieceRequirements = (props: { prefix: string; values: { PieceRequireAll: number[]; PieceRequireAny: number[]; PieceRequireNone: number[] }; disabled: boolean }) => {
  const t = useSetupTranslate();

  return (
    <>
      <RequirementsEditor field={`${props.prefix}.PieceRequireAll`} label={t("PieceRequireAll", "Only with all of")} values={props.values.PieceRequireAll} disabled={props.disabled} />
      <RequirementsEditor field={`${props.prefix}.PieceRequireAny`} label={t("PieceRequireAny", "Only with any of")} values={props.values.PieceRequireAny} disabled={props.disabled} />
      <RequirementsEditor field={`${props.prefix}.PieceRequireNone`} label={t("PieceRequireNone", "Never with")} values={props.values.PieceRequireNone} disabled={props.disabled} />
    </>
  );
};

const optionTypeNames: Record<LaneOptionType, string> = {
  [LaneOptionType.SingleSelectionButtons]: "Buttons",
  [LaneOptionType.MultiSelectionButtons]: "Multi-select buttons",
  [LaneOptionType.Toggle]: "Toggle",
  [LaneOptionType.Checkbox]: "Checkbox",
  [LaneOptionType.ValueUpDown]: "Up/down value",
  [LaneOptionType.LaneWidth]: "Lane width",
  [LaneOptionType.Decoration]: "Grass & trees",
  [LaneOptionType.TwoWay]: "Two-way",
};

const findValue = (combination: LaneSetupCombination[], option?: string) =>
  combination.find((x) => (x.Option ?? "").toLowerCase() === (option ?? "").toLowerCase())?.Value;

// Which option values this section stands for inside its group
export const VariantEditor = (props: { state: LaneSetupState; disabled: boolean }) => {
  const t = useSetupTranslate();
  const groups = useValue(laneSetupGroups$);
  const variant = props.state.Variant;
  const group = groups.find((x) => x.Name === variant?.Group);
  const combination = variant?.Combination ?? [];

  return (
    <>
      <Row label={t("Group", "Lane group")} hint={t("GroupHint", "The lane in Road Builder's list that this section is one variant of.")}>
        <Picker
          value={variant?.Group}
          placeholder={t("PickGroup", "Pick a lane group…")}
          disabled={props.disabled}
          items={groups.map((x) => ({ value: x.Name, label: x.DisplayName, detail: x.BuiltIn ? t("BuiltIn", "Road Builder") : x.Name }))}
          onSelect={(value) => setLaneSetupText("Variant.Group", value)}
        />
      </Row>
      {group &&
        group.Options.map((option, i) => {
          const value = findValue(combination, option.Name);
          const name = option.Name ?? "";

          if (option.Type === LaneOptionType.Checkbox || option.Type === LaneOptionType.TwoWay) {
            return (
              <Toggle
                key={i}
                checked={!!value}
                disabled={props.disabled}
                label={option.Type === LaneOptionType.TwoWay ? t("TwoWayVariant", "This is the two-way variant") : name}
                hint={option.Type === LaneOptionType.TwoWay ? undefined : t("CheckboxVariant", "Checked when the option is on")}
                onChange={(checked) => setLaneSetupCombination(name, checked ? (option.Type === LaneOptionType.TwoWay ? "1" : "checked") : "")}
              />
            );
          }

          if (option.Type === LaneOptionType.Decoration) {
            return (
              <Row key={i} label={t("Decorations", "Grass & trees")} hint={t("DecorationVariantHint", "Which decorations this variant supports.")}>
                <Chips
                  disabled={props.disabled}
                  items={[
                    { value: "", label: t("None", "None") },
                    { value: "G", label: t("GrassOnly", "Grass") },
                    { value: "T", label: t("TreesOnly", "Trees") },
                    { value: "GT", label: t("GrassAndTrees", "Both") },
                  ]}
                  isSelected={(x) => (value ?? "") === x}
                  onSelect={(x) => setLaneSetupCombination(name, x)}
                />
              </Row>
            );
          }

          return (
            <Row key={i} label={name || t("Unnamed", "Unnamed option")}>
              <Chips
                disabled={props.disabled}
                items={[{ value: "", label: t("None", "None") }, ...option.Values.map((x) => ({ value: x.Value ?? "", label: x.Value || "—", icon: x.Thumbnail }))]}
                isSelected={(x) => (value ?? "").toLowerCase() === x.toLowerCase()}
                onSelect={(x) => setLaneSetupCombination(name, x)}
              />
            </Row>
          );
        })}
    </>
  );
};

// The options a lane group shows in Road Builder, and the sections that are its variants
export const GroupOptionsEditor = (props: { state: LaneSetupState; disabled: boolean }) => {
  const t = useSetupTranslate();
  const options = props.state.Group?.Options ?? [];
  const linked = props.state.Group?.LinkedSections ?? [];
  const sections = useValue(laneSetupSections$);
  const [variantSource, setVariantSource] = useState<string>();

  return (
    <>
      {options.length === 0 && (
        <p className={styles.description}>{t("NoOptions", "No options yet. Add one for each choice players make, like the lane's width or whether it has markings.")}</p>
      )}
      {options.map((option, index) => (
        <OptionEditor key={index} option={option} index={index} count={options.length} disabled={props.disabled} />
      ))}
      <Button variant="flat" className={styles.wideButton} disabled={props.disabled} onSelect={props.disabled ? undefined : addLaneSetupOption}>
        {t("AddOption", "Add option")}
      </Button>
      <p className={classNames(styles.hint, styles.spaced)}>
        {t("OptionOrderHint", "Options are shown in this order, and each option only offers the values that still have a variant after the options above it.")}
      </p>

      <div className={styles.subTitle}>{t("Variants", "Variants")}</div>
      <p className={styles.description}>
        {linked.length === 0
          ? t("NoVariants", "No variants yet. Select a net section, set its lane type to Group variant, and pick this group.")
          : t("VariantsHint", "The sections players get for each combination of values. Click one to select it in the inspector.")}
      </p>
      {linked.map((section) => (
        <div key={section.Name} className={styles.linkedSection}>
          <div className={styles.linkedName} onClick={() => section.Name && selectLaneSetupPrefab(section.Name)}>
            {section.DisplayName ?? section.Name}
          </div>
          <div className={styles.hint}>{section.Combination.map((x) => `${x.Option}: ${x.Value || "—"}`).join("  ·  ") || t("NoValues", "No option values set")}</div>
        </div>
      ))}

      <div className={styles.spaced}>
        <Row label={t("NewVariant", "New variant")} hint={t("NewVariantHint", "Copies a net section as a variant of this group and selects it, so you can set its option values.")}>
          <Picker
            value={variantSource}
            placeholder={t("PickSection", "Pick a net section…")}
            disabled={props.disabled}
            items={sections.map((x) => ({ value: x, label: x }))}
            onSelect={(v) => setVariantSource(v || undefined)}
          />
        </Row>
        <Button
          variant="flat"
          className={styles.smallButton}
          disabled={props.disabled || !variantSource || !props.state.Name}
          onSelect={props.disabled || !variantSource || !props.state.Name ? undefined : () => createLaneSetupFromSection(variantSource, "variant", props.state.Name)}
        >
          {t("CreateVariant", "Create variant")}
        </Button>
      </div>
    </>
  );
};

// What each option type is for, shown under the type picker
const optionTypeHints: Record<LaneOptionType, string> = {
  [LaneOptionType.SingleSelectionButtons]: "A row of buttons, one per value. Players pick one.",
  [LaneOptionType.MultiSelectionButtons]: "A row of buttons, one per value. Currently works the same as Buttons.",
  [LaneOptionType.Toggle]: "Buttons joined into a single toggle, for a few short choices.",
  [LaneOptionType.Checkbox]: "A single on/off checkbox. Doesn't need values.",
  [LaneOptionType.ValueUpDown]: "Arrows that step through the values in order.",
  [LaneOptionType.LaneWidth]: "Arrows that step through widths in order, shown in feet for imperial users.",
  [LaneOptionType.Decoration]: "Grass and tree toggles. Doesn't pick a variant, each variant says which decorations it supports.",
  [LaneOptionType.TwoWay]: "Adds a Two-way button to the lane's direction. One variant is the two-way version, the others are one-way.",
};

// How variants use the option types that have no list of values
const noValuesHints: Partial<Record<LaneOptionType, string>> = {
  [LaneOptionType.Checkbox]: "Variants that are the checked version set a value for this option, the unchecked ones leave it out.",
  [LaneOptionType.Decoration]: "Set which decorations each variant supports in the variant's own setup.",
  [LaneOptionType.TwoWay]: "Mark the two-way variant in its own setup, the variants without it are used for one-way lanes.",
};

const OptionEditor = (props: { option: LaneSetupOption; index: number; count: number; disabled: boolean }) => {
  const t = useSetupTranslate();
  const { option, index, disabled } = props;
  const hasValues = valueOptionTypes.includes(option.Type);
  const isWidth = option.Type === LaneOptionType.LaneWidth;
  const noValuesHint = noValuesHints[option.Type];

  return (
    <div className={styles.optionCard}>
      <div className={styles.optionHeader}>
        <span className={styles.optionNumber}>{t("OptionNumber", "Option {0}").replace("{0}", (index + 1).toString())}</span>
        <IconButton icon="Media/Glyphs/ThickStrokeArrowUp.svg" tooltip={t("MoveUp", "Move up")} disabled={disabled || index === 0} onSelect={() => moveLaneSetupOption(index, -1)} />
        <IconButton
          icon="Media/Glyphs/ThickStrokeArrowDown.svg"
          tooltip={t("MoveDown", "Move down")}
          disabled={disabled || index === props.count - 1}
          onSelect={() => moveLaneSetupOption(index, 1)}
        />
        <IconButton danger icon="Media/Glyphs/Trash.svg" tooltip={t("RemoveOption", "Remove option")} disabled={disabled} onSelect={() => removeLaneSetupOption(index)} />
      </div>

      <Row label={t("OptionName", "Name")} hint={t("OptionNameHint", "Shown above the option in Road Builder. Variants refer to the option by this name, so avoid renaming it once published.")}>
        <TextField value={option.Name} placeholder={t("OptionNamePlaceholder", "e.g. Width, Markings, Parking")} disabled={disabled} onCommit={(v) => setLaneSetupOption(index, "Name", v)} />
      </Row>

      <Row label={t("OptionType", "Type")} hint={t(`OptionTypeHint[${option.Type}]`, optionTypeHints[option.Type] ?? "")}>
        <Chips
          disabled={disabled}
          items={Object.entries(optionTypeNames).map(([value, label]) => ({ value: parseInt(value) as LaneOptionType, label: t(`OptionType[${value}]`, label) }))}
          isSelected={(x) => x === option.Type}
          onSelect={(x) => setLaneSetupOption(index, "Type", x.toString())}
        />
      </Row>

      {hasValues && (
        <>
          <div className={styles.subTitle}>{t("Values", "Values")}</div>
          <p className={styles.description}>
            {isWidth
              ? t("WidthValuesHint", "One value per width, from narrowest to widest, written in meters like 3m or 3.5m.")
              : t("ValuesHint", "One value per choice. Each variant picks one of these, so keep them short and don't rename them once published.")}
          </p>

          {option.Values.length === 0 && <p className={styles.description}>{t("NoOptionValues", "No values yet.")}</p>}

          {option.Values.map((value, valueIndex) => (
            <div key={valueIndex} className={styles.optionValue}>
              <span className={styles.valueLabel}>{t("ValueNumber", "Value {0}").replace("{0}", (valueIndex + 1).toString())}</span>
              <TextField
                value={value.Value}
                placeholder={isWidth ? t("WidthPlaceholder", "e.g. 3.5m") : t("ValuePlaceholder", "e.g. Painted, None, Angled")}
                disabled={disabled}
                onCommit={(v) => setLaneSetupOptionValue(index, valueIndex, "Value", v)}
              />
              <div className={styles.valueIconLabel}>
                <span>{t("ButtonIcon", "Button icon")}</span>
                <span className={styles.hint}>{t("ButtonIconHint", "Without an icon, the button shows the value's name.")}</span>
              </div>
              <ImageField
                value={value.Thumbnail}
                field={`Option.${index}`}
                index={valueIndex}
                disabled={disabled}
                onCommit={(v) => setLaneSetupOptionValue(index, valueIndex, "Thumbnail", v)}
                onRemove={() => removeLaneSetupOptionValue(index, valueIndex)}
              />
            </div>
          ))}
          <Button variant="flat" className={styles.smallButton} disabled={disabled} onSelect={disabled ? undefined : () => addLaneSetupOptionValue(index)}>
            {t("AddValue", "Add value")}
          </Button>

          {option.Values.length > 0 && (
            <div className={styles.spaced}>
              <Row label={t("DefaultValue", "Default value")} hint={t("DefaultValueHint", "Selected when the lane is first added to a road.")}>
                <Chips
                  disabled={disabled}
                  items={option.Values.map((x) => ({ value: x.Value ?? "", label: x.Value || "—" }))}
                  isSelected={(x) => x === (option.DefaultValue ?? "")}
                  onSelect={(x) => setLaneSetupOption(index, "DefaultValue", x)}
                />
              </Row>
            </div>
          )}
        </>
      )}

      {!hasValues && noValuesHint && <p className={styles.description}>{t(`NoValuesHint[${option.Type}]`, noValuesHint)}</p>}

      <Toggle
        checked={option.IgnoreForSimilarDuplicate}
        disabled={disabled}
        label={t("IgnoreForDuplicate", "Don't copy to neighboring lanes")}
        hint={t("IgnoreForDuplicateHint", "When a similar lane is added next to this one, it starts with the default instead of this lane's value.")}
        onChange={(v) => setLaneSetupOption(index, "IgnoreForSimilarDuplicate", v ? "true" : "false")}
      />
    </div>
  );
};

// Extra sections placed on either side of the lane, like the parking lane's shoulder
export const AggregateEditor = (props: { left: LaneSetupAggregateSection[]; right: LaneSetupAggregateSection[]; disabled: boolean }) => {
  const t = useSetupTranslate();

  const side = (name: "Left" | "Right", sections: LaneSetupAggregateSection[]) => (
    <>
      <div className={styles.subTitle}>{name === "Left" ? t("LeftSections", "Left of the lane") : t("RightSections", "Right of the lane")}</div>
      {sections.map((section, index) => (
        <AggregateSectionEditor key={index} side={name} index={index} section={section} disabled={props.disabled} />
      ))}
      <Button variant="flat" className={styles.smallButton} disabled={props.disabled} onSelect={props.disabled ? undefined : () => addLaneSetupAggregateSection(name)}>
        {t("AddSection", "Add section")}
      </Button>
    </>
  );

  return (
    <>
      {side("Left", props.left)}
      {side("Right", props.right)}
    </>
  );
};

const AggregateSectionEditor = (props: { side: "Left" | "Right"; index: number; section: LaneSetupAggregateSection; disabled: boolean }) => {
  const t = useSetupTranslate();
  const sections = useValue(laneSetupSections$);
  const [showRequirements, setShowRequirements] = useState(false);
  const { side, index, section, disabled } = props;

  return (
    <div className={styles.optionCard}>
      <div className={styles.optionHeader}>
        <Picker
          value={section.Section}
          placeholder={t("PickSection", "Pick a net section…")}
          disabled={disabled}
          items={sections.map((x) => ({ value: x, label: x }))}
          onSelect={(v) => setLaneSetupAggregateSection(side, index, "Section", v)}
        />
        <IconButton danger icon="Media/Glyphs/Trash.svg" tooltip={t("Remove", "Remove")} disabled={disabled} onSelect={() => removeLaneSetupAggregateSection(side, index)} />
      </div>
      <Toggle
        checked={section.Invert}
        disabled={disabled}
        label={t("InvertSection", "Flip relative to the lane")}
        onChange={(v) => setLaneSetupAggregateSection(side, index, "Invert", v ? "true" : "false")}
      />
      <Toggle checked={showRequirements} label={t("ShowRequirements", "Only in some states (requirements)")} onChange={setShowRequirements} />
      {showRequirements && <PieceRequirements prefix={`Aggregate.${side}.${index}`} values={section} disabled={disabled} />}
    </div>
  );
};

export const DecorationEditor = (props: { decoration: LaneSetupDecoration; disabled: boolean }) => {
  const t = useSetupTranslate();
  const { decoration, disabled } = props;

  return (
    <>
      <Row label={t("GrassIcon", "Lane icon with grass")}>
        <ImageField value={decoration.GrassThumbnail} field="Decoration.GrassThumbnail" disabled={disabled} onCommit={(v) => setLaneSetupText("Decoration.GrassThumbnail", v)} />
      </Row>
      <Row label={t("TreeIcon", "Lane icon with trees")}>
        <ImageField value={decoration.TreeThumbnail} field="Decoration.TreeThumbnail" disabled={disabled} onCommit={(v) => setLaneSetupText("Decoration.TreeThumbnail", v)} />
      </Row>
      <Row label={t("GrassAndTreeIcon", "Lane icon with both")}>
        <ImageField
          value={decoration.GrassAndTreeThumbnail}
          field="Decoration.GrassAndTreeThumbnail"
          disabled={disabled}
          onCommit={(v) => setLaneSetupText("Decoration.GrassAndTreeThumbnail", v)}
        />
      </Row>
      <Row label={t("GrassRoadThumbnail", "Road thumbnail with grass")}>
        <ImageListField values={decoration.LaneGrassThumbnail} field="Decoration.LaneGrassThumbnail" disabled={disabled} onCommit={(v) => setLaneSetupList("Decoration.LaneGrassThumbnail", v)} />
      </Row>
      <Row label={t("TreeRoadThumbnail", "Road thumbnail with trees")}>
        <ImageListField values={decoration.LaneTreeThumbnail} field="Decoration.LaneTreeThumbnail" disabled={disabled} onCommit={(v) => setLaneSetupList("Decoration.LaneTreeThumbnail", v)} />
      </Row>
      <Row label={t("GrassAndTreeRoadThumbnail", "Road thumbnail with both")}>
        <ImageListField
          values={decoration.LaneGrassAndTreeThumbnail}
          field="Decoration.LaneGrassAndTreeThumbnail"
          disabled={disabled}
          onCommit={(v) => setLaneSetupList("Decoration.LaneGrassAndTreeThumbnail", v)}
        />
      </Row>
    </>
  );
};

export const SidePicker = (props: { label: string; field: "Edge.LeftSide" | "Edge.RightSide"; value?: string; disabled: boolean }) => {
  const t = useSetupTranslate();
  const sections = useValue(laneSetupSections$);

  return (
    <Row label={props.label}>
      <Picker
        value={props.value}
        allowClear
        placeholder={t("DefaultSide", "Road's default side")}
        disabled={props.disabled}
        items={sections.map((x) => ({ value: x, label: x }))}
        onSelect={(v) => setLaneSetupText(props.field, v)}
      />
    </Row>
  );
};

// Messages come from LaneSetupValidator in C#, some of them name the option or section involved
export const IssueList = (props: { state: LaneSetupState }) => {
  if (props.state.Issues.length === 0) {
    return null;
  }

  return (
    <div className={styles.issues}>
      {props.state.Issues.map((issue, i) => (
        <div key={i} className={classNames(styles.issue, styles[issue.Severity])}>
          {issue.Severity === "info" ? <div className={styles.dot} /> : <img style={{ maskImage: "url(Media/Misc/Warning.svg)" }} />}
          <span>{issue.Message}</span>
        </div>
      ))}
    </div>
  );
};
