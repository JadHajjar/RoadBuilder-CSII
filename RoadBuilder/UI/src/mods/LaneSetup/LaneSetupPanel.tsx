import classNames from "classnames";
import { Button, Scrollable } from "cs2/ui";
import { useEffect, useState } from "react";
import { LaneGroundType, LaneSetupLaneInfo, LaneSetupRole, LaneSetupState } from "domain/LaneSetup";
import { RoadButtonSmall } from "mods/Components/RoadButtonSmall/RoadButtonSmall";
import { AggregateEditor, CategoryEditor, DecorationEditor, GroupOptionsEditor, IssueList, PieceRequirements, SidePicker, VariantEditor } from "./Editors";
import { Card, Chips, ImageField, ImageListField, Row, TextField, Toggle, useSetupTranslate } from "./Fields";
import { GettingStarted } from "./GettingStarted";
import {
  createLaneSetupFromSection,
  setLaneSetupEnabled,
  setLaneSetupFlag,
  setLaneSetupList,
  setLaneSetupNumber,
  setLaneSetupOpen,
  setLaneSetupRole,
  setLaneSetupText,
} from "./laneSetupBindings";
import styles from "./LaneSetup.module.scss";

// Colors used by Road Builder's own lanes, offered as a starting point
const colorPresets = [
  { label: "Bike", value: "#2BA152C8" },
  { label: "Bus", value: "#B23C3CC8" },
  { label: "Harbor", value: "#919BA396" },
  { label: "Gravel", value: "#8F8361FF" },
  { label: "Tiled", value: "#4C4E53FF" },
];

export const LaneSetupPanel = (props: { state: LaneSetupState }) => {
  const t = useSetupTranslate();
  const { state } = props;
  const disabled = state.ReadOnly;

  return (
    <div className={styles.panel}>
      <div className={styles.header}>
        <img className={styles.headerIcon} src="coui://roadbuildericons/EditRoadBuilder.svg" />
        <div className={styles.headerText}>
          <span className={styles.headerTitle}>{t("Title", "Road Builder Setup")}</span>
          <span className={styles.headerSubtitle}>{state.Available ? state.DisplayName ?? state.Name : t("GettingStarted", "Getting started")}</span>
        </div>
        <Button variant="flat" className={styles.closeButton} onSelect={() => setLaneSetupOpen(false)}>
          <img style={{ maskImage: "url(Media/Glyphs/Close.svg)" }} />
        </Button>
      </div>

      <Scrollable className={styles.content} vertical smooth trackVisibility="scrollable">
        {!state.Available ? <GettingStarted /> : <SelectedPrefabSetup state={state} disabled={disabled} />}
      </Scrollable>
    </div>
  );
};

const SelectedPrefabSetup = (props: { state: LaneSetupState; disabled: boolean }) => {
  const t = useSetupTranslate();
  const { state, disabled } = props;

  return (
    <>
        {state.ReadOnly && (
          <div className={styles.banner}>
            {state.ReadOnlyReason === "BuiltIn"
              ? t("ReadOnlyBuiltIn", "This lane is part of Road Builder and can't be edited here. Make a copy to create your own version of it.")
              : t("ReadOnly", "This prefab is read-only. Make a copy to set it up for Road Builder.")}
            {!state.IsGroup && state.Name && (
              <Button
                variant="flat"
                className={classNames(styles.smallButton, styles.bannerButton)}
                onSelect={() => createLaneSetupFromSection(state.Name!, state.Role === "edge" ? "edge" : "lane")}
              >
                {t("MakeEditableCopy", "Make an editable copy")}
              </Button>
            )}
          </div>
        )}

        <IssueList state={state} />

        {state.Preview && state.Preview.length > 0 && <Preview state={state} />}

        {/* Keyed by prefab, so local state like advanced toggles resets when the inspector selects another prefab */}
        {state.IsGroup ? <GroupSetup key={state.Name} state={state} disabled={disabled} /> : <SectionSetup key={state.Name} state={state} disabled={disabled} />}
    </>
  );
};

const Preview = (props: { state: LaneSetupState }) => {
  const t = useSetupTranslate();
  const lanes = props.state.Preview ?? [];
  const section = lanes[0]?.NetSection;

  return (
    <Card title={t("Preview", "Preview")}>
      <div className={styles.previewTile}>
        <img src={section?.Thumbnail || "coui://roadbuildericons/RB_Unknown.svg"} />
        <div>
          <span>{section?.DisplayName}</span>
          <span className={styles.hint}>{section?.WidthText}</span>
        </div>
      </div>
      <div className={styles.previewLanes}>
        {lanes.map((lane, i) => (
          <RoadButtonSmall key={i} roadLane={lane} index={i} onDelete={() => {}} />
        ))}
      </div>
    </Card>
  );
};

const RoleSelector = (props: { role: LaneSetupRole; disabled: boolean }) => {
  const t = useSetupTranslate();
  const [confirmRemove, setConfirmRemove] = useState(false);

  useEffect(() => setConfirmRemove(false), [props.role]);

  const roles: { role: LaneSetupRole; label: string; description: string }[] = [
    { role: "lane", label: t("RoleLane", "Lane"), description: t("RoleLaneHint", "Shows up on its own in Road Builder's lane list.") },
    { role: "edge", label: t("RoleEdge", "Edge lane"), description: t("RoleEdgeHint", "A sidewalk or shoulder, kept on the outer edges of the road.") },
    { role: "variant", label: t("RoleVariant", "Group variant"), description: t("RoleVariantHint", "One version of a lane group, picked through the group's options.") },
  ];

  return (
    <Card title={t("LaneType", "Lane type")}>
      <div className={styles.roles}>
        {roles.map((x) => (
          <div
            key={x.role}
            className={classNames(styles.role, props.role === x.role && styles.selected, props.disabled && styles.disabled)}
            onClick={props.disabled ? undefined : () => setLaneSetupRole(x.role)}
          >
            <span className={styles.roleLabel}>{x.label}</span>
            <span className={styles.hint}>{x.description}</span>
          </div>
        ))}
      </div>
      {props.role === "none" ? (
        <p className={styles.description}>{t("NotSetUp", "Pick a lane type to make this section available in Road Builder.")}</p>
      ) : (
        <Button
          variant="flat"
          className={classNames(styles.smallButton, styles.danger)}
          disabled={props.disabled}
          onSelect={
            props.disabled
              ? undefined
              : () => {
                  if (confirmRemove) setLaneSetupRole("none");
                  else setConfirmRemove(true);
                }
          }
        >
          {confirmRemove ? t("ConfirmRemoveSetup", "Click again to remove all Road Builder components") : t("RemoveSetup", "Remove from Road Builder")}
        </Button>
      )}
    </Card>
  );
};

const SectionSetup = (props: { state: LaneSetupState; disabled: boolean }) => {
  const t = useSetupTranslate();
  const { state, disabled } = props;
  const isVariant = state.Role === "variant";

  return (
    <>
      <RoleSelector role={state.Role} disabled={disabled} />

      {isVariant && (
        <Card title={t("GroupCard", "Lane group")} description={t("GroupCardHint", "Pick the group and the option values that lead to this section.")}>
          <VariantEditor state={state} disabled={disabled} />
        </Card>
      )}

      {isVariant && (
        <Card title={t("VariantOverrides", "Variant icon & road types")}>
          <Toggle
            checked={!!state.LaneInfo}
            disabled={disabled}
            label={t("VariantOwnInfo", "Use its own icon and road types")}
            hint={t("VariantOwnInfoHint", "Otherwise the group's settings are used.")}
            onChange={(v) => setLaneSetupEnabled("LaneInfo", v)}
          />
        </Card>
      )}

      {(state.Role === "lane" || state.Role === "edge") && <ListIconCard state={state} disabled={disabled} />}

      {state.LaneInfo && state.Role !== "none" && <LaneInfoCards info={state.LaneInfo} disabled={disabled} />}

      {state.Role === "edge" && state.Edge && <EdgeCard state={state} disabled={disabled} />}

      {state.Role !== "none" && <AdvancedCard state={state} disabled={disabled} />}
    </>
  );
};

const GroupSetup = (props: { state: LaneSetupState; disabled: boolean }) => {
  const t = useSetupTranslate();
  const { state, disabled } = props;

  return (
    <>
      <Card title={t("Options", "Options")} description={t("OptionsHint", "The choices shown when the lane is selected in Road Builder. Each variant section matches one combination of values.")}>
        <GroupOptionsEditor state={state} disabled={disabled} />
      </Card>

      <Card title={t("GroupSettings", "Group icon & road types")}>
        <Toggle
          checked={!!state.LaneInfo}
          disabled={disabled}
          label={t("GroupOwnInfo", "Set an icon and road types for the whole group")}
          hint={t("GroupOwnInfoHint", "Recommended, the lane list uses these for the group's entry.")}
          onChange={(v) => setLaneSetupEnabled("LaneInfo", v)}
        />
        <Toggle
          checked={!!state.Edge}
          disabled={disabled}
          label={t("GroupIsEdge", "The group is an edge lane")}
          onChange={(v) => setLaneSetupEnabled("Edge", v)}
        />
      </Card>

      <ListIconCard state={state} disabled={disabled} />

      {state.LaneInfo && <LaneInfoCards info={state.LaneInfo} disabled={disabled} />}

      {state.Edge && <EdgeCard state={state} disabled={disabled} />}

      <AdvancedCard state={state} disabled={disabled} />
    </>
  );
};

const LaneInfoCards = (props: { info: LaneSetupLaneInfo; disabled: boolean }) => {
  const t = useSetupTranslate();
  const { info, disabled } = props;
  const [separateBack, setSeparateBack] = useState(!!info.BackThumbnail && info.BackThumbnail !== info.FrontThumbnail);

  const setIcon = (value: string) => {
    setLaneSetupText("LaneInfo.FrontThumbnail", value);
    if (!separateBack) setLaneSetupText("LaneInfo.BackThumbnail", value);
  };

  return (
    <>
      <Card title={t("Availability", "Road types")}>
        <CategoryEditor info={info} disabled={disabled} />
      </Card>

      <Card title={t("Appearance", "Appearance")}>
        <Row label={separateBack ? t("FrontIcon", "Icon facing forward") : t("Icon", "Lane icon")} hint={t("IconHint", "Shown above the lane in the lane bar.")}>
          <ImageField value={info.FrontThumbnail} field="LaneInfo.FrontThumbnail" disabled={disabled} onCommit={setIcon} />
        </Row>
        {separateBack && (
          <Row label={t("BackIcon", "Icon facing backward")}>
            <ImageField value={info.BackThumbnail} field="LaneInfo.BackThumbnail" disabled={disabled} onCommit={(v) => setLaneSetupText("LaneInfo.BackThumbnail", v)} />
          </Row>
        )}
        <Toggle
          checked={separateBack}
          disabled={disabled}
          label={t("SeparateBack", "Different icon when facing backward")}
          onChange={(v) => {
            setSeparateBack(v);
            if (!v) setLaneSetupText("LaneInfo.BackThumbnail", info.FrontThumbnail ?? "");
          }}
        />

        <Row label={t("Color", "Lane color")} hint={t("ColorHint", "Tints the lane in the lane bar, leave empty for the road's surface color.")}>
          <div className={styles.colorRow}>
            <div className={styles.swatch} style={{ backgroundColor: info.Color ?? "transparent" }} />
            <TextField value={info.Color} placeholder="#RRGGBBAA" disabled={disabled} onCommit={(v) => setLaneSetupText("LaneInfo.Color", v)} />
          </div>
          <Chips
            disabled={disabled}
            items={[{ value: "", label: t("NoColor", "None") }, ...colorPresets.map((x) => ({ value: x.value, label: t(`ColorPreset[${x.label}]`, x.label) }))]}
            isSelected={(x) => (info.Color ?? "").toUpperCase() === x.toUpperCase()}
            onSelect={(x) => setLaneSetupText("LaneInfo.Color", x)}
          />
        </Row>

        <Row label={t("GroundTexture", "Lane bar surface")}>
          <Chips
            disabled={disabled}
            items={[
              { value: LaneGroundType.Unset, label: t("SurfaceAuto", "Automatic") },
              { value: LaneGroundType.Asphalt, label: t("SurfaceAsphalt", "Asphalt") },
              { value: LaneGroundType.PaintedAsphalt, label: t("SurfacePainted", "Painted") },
              { value: LaneGroundType.Gravel, label: t("SurfaceGravel", "Gravel") },
              { value: LaneGroundType.Tiled, label: t("SurfaceTiled", "Tiled") },
              { value: LaneGroundType.Train, label: t("SurfaceTrain", "Rail") },
            ]}
            isSelected={(x) => x === info.GroundTexture}
            onSelect={(x) => setLaneSetupNumber("LaneInfo.GroundTexture", x)}
          />
        </Row>

        <Toggle
          checked={info.NoDirection}
          disabled={disabled}
          label={t("NoDirection", "No direction")}
          hint={t("NoDirectionHint", "Hides the direction arrow, for lanes like medians, sidewalks or parking.")}
          onChange={(v) => setLaneSetupFlag("LaneInfo.NoDirection", v)}
        />
        <Toggle
          checked={info.Median}
          disabled={disabled}
          label={t("Median", "Median")}
          hint={t("MedianHint", "Marks the section as a median in the generated road.")}
          onChange={(v) => setLaneSetupFlag("LaneInfo.Median", v)}
        />
      </Card>

      <Card
        title={t("RoadThumbnail", "Road thumbnail")}
        description={t("RoadThumbnailHint", "Images drawn for this lane in the generated road's thumbnail, from left to right.")}
        collapsible
        defaultOpen={info.LaneThumbnails.length > 0}
      >
        <ImageListField values={info.LaneThumbnails} field="LaneInfo.LaneThumbnails" disabled={disabled} onCommit={(v) => setLaneSetupList("LaneInfo.LaneThumbnails", v)} />
      </Card>
    </>
  );
};

const EdgeCard = (props: { state: LaneSetupState; disabled: boolean }) => {
  const t = useSetupTranslate();
  const edge = props.state.Edge!;
  const { disabled } = props;

  return (
    <Card title={t("EdgeLane", "Edge lane")} description={t("EdgeLaneHint", "Edge lanes sit on the outer edges of the road. The sides are the sections added beyond the lane, like curbs.")}>
      <SidePicker label={t("LeftSide", "Left side section")} field="Edge.LeftSide" value={edge.LeftSide} disabled={disabled} />
      <SidePicker label={t("RightSide", "Right side section")} field="Edge.RightSide" value={edge.RightSide} disabled={disabled} />
      <Toggle
        checked={edge.AddSidewalkStateOnNode}
        disabled={disabled}
        label={t("SidewalkState", "Has a sidewalk")}
        hint={t("SidewalkStateHint", "Gives intersections the sidewalk corners and crosswalks of a road with sidewalks.")}
        onChange={(v) => setLaneSetupFlag("Edge.AddSidewalkStateOnNode", v)}
      />
      <Toggle
        checked={edge.DoNotRequireBeingOnEdge}
        disabled={disabled}
        label={t("AnywhereOnRoad", "Can also be placed between lanes")}
        hint={t("AnywhereOnRoadHint", "Allows the lane away from the road's edges, like a sound barrier.")}
        onChange={(v) => setLaneSetupFlag("Edge.DoNotRequireBeingOnEdge", v)}
      />
    </Card>
  );
};

const AdvancedCard = (props: { state: LaneSetupState; disabled: boolean }) => {
  const t = useSetupTranslate();
  const { state, disabled } = props;
  const showDecoration = !!state.Decoration || state.IsGroup || state.Role === "variant";

  return (
    <Card title={t("Advanced", "Advanced")} collapsible defaultOpen={!!state.Aggregate || !!state.Decoration}>
      <Toggle
        checked={!!state.Aggregate}
        disabled={disabled}
        label={t("ExtraSections", "Add extra sections around the lane")}
        hint={t("ExtraSectionsHint", "Places other sections next to this lane in the generated road, like the shoulder next to a parking lane.")}
        onChange={(v) => setLaneSetupEnabled("Aggregate", v)}
      />
      {state.Aggregate && <AggregateEditor left={state.Aggregate.Left} right={state.Aggregate.Right} disabled={disabled} />}

      {showDecoration && (
        <>
          <Toggle
            checked={!!state.Decoration}
            disabled={disabled}
            label={t("DecorationIcons", "Icons for grass & trees")}
            hint={t("DecorationIconsHint", "Used when the group has a grass & trees option and it's turned on.")}
            onChange={(v) => setLaneSetupEnabled("Decoration", v)}
          />
          {state.Decoration && <DecorationEditor decoration={state.Decoration} disabled={disabled} />}
        </>
      )}

      {state.LaneInfo && (
        <>
          <div className={styles.subTitle}>{t("Requirements", "Only in some road states")}</div>
          <p className={styles.description}>{t("RequirementsHint", "Limits the section to edges in a given state, like elevated or with an upgrade. Most lanes don't need this.")}</p>
          <PieceRequirements prefix="LaneInfo" values={state.LaneInfo} disabled={disabled} />
        </>
      )}

      {!state.IsGroup && (
        <Toggle
          checked={state.Ignore}
          disabled={disabled}
          label={t("IgnoreSection", "Skip when importing roads")}
          hint={t("IgnoreSectionHint", "Leaves this section out when a road that uses it is opened in Road Builder.")}
          onChange={(v) => setLaneSetupEnabled("Ignore", v)}
        />
      )}
    </Card>
  );
};

// The prefab's own icon, which is what Road Builder's lane list shows for the lane or group
const ListIconCard = (props: { state: LaneSetupState; disabled: boolean }) => {
  const t = useSetupTranslate();

  return (
    <Card title={t("LaneList", "Lane list")}>
      <Row label={t("ListIcon", "List icon")} hint={t("ListIconHint", "Shown for the lane in Road Builder's lane list. This is the prefab's own icon, the lane bar uses the icons below.")}>
        <ImageField value={props.state.ListIcon} field="Prefab.ListIcon" disabled={props.disabled} onCommit={(v) => setLaneSetupText("Prefab.ListIcon", v)} />
      </Row>
    </Card>
  );
};
