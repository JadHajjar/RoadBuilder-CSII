import { useValue } from "cs2/api";
import { Button } from "cs2/ui";
import { useState } from "react";
import { Card, Picker, Row, TextField, useSetupTranslate } from "./Fields";
import { createLaneSetupFromSection, createLaneSetupGroup, laneSetupGroups$, laneSetupSections$ } from "./laneSetupBindings";
import styles from "./LaneSetup.module.scss";

// Shown when nothing is selected: how the prefabs and components fit together, and shortcuts to start new ones
export const GettingStarted = () => {
  const t = useSetupTranslate();
  const sections = useValue(laneSetupSections$);
  const [source, setSource] = useState<string>();
  const [groupName, setGroupName] = useState("");
  const groups = useValue(laneSetupGroups$);
  // Same rule as the C# side: a name is required and can't match an existing group, ignoring case
  const trimmedName = groupName.trim();
  const nameTaken = trimmedName !== "" && groups.some((x) => x.Name.toLowerCase() === trimmedName.toLowerCase());
  const canCreateGroup = trimmedName !== "" && !nameTaken;

  const concepts = [
    {
      title: t("ConceptLane", "Lane"),
      text: t("ConceptLaneText", "A net section with Lane info. It appears on its own in Road Builder's lane list, and is all most custom lanes need."),
    },
    {
      title: t("ConceptEdge", "Edge lane"),
      text: t("ConceptEdgeText", "A lane that also has Edge lane info, like a sidewalk or shoulder. It's kept on the outer edges of the road and decides what goes beyond it."),
    },
    {
      title: t("ConceptGroup", "Lane group"),
      text: t("ConceptGroupText", "A separate lane group prefab that bundles several sections. Players see one lane and choose between its versions with options, like width or markings."),
    },
    {
      title: t("ConceptVariant", "Group variant"),
      text: t("ConceptVariantText", "A net section linked to a lane group, with the option values that lead to it. It's only reachable through its group."),
    },
  ];

  return (
    <>
      <Card title={t("HowItWorks", "How custom lanes work")}>
        <p className={styles.description}>
          {t(
            "HowItWorksText",
            "Every lane in Road Builder is a net section. Road Builder components on the section tell Road Builder how to show it, which roads it can be used in, and how it fits into the road."
          )}
        </p>
        <div className={styles.concepts}>
          {concepts.map((x) => (
            <div key={x.title} className={styles.concept}>
              <span className={styles.conceptTitle}>{x.title}</span>
              <span className={styles.hint}>{x.text}</span>
            </div>
          ))}
        </div>
        <p className={styles.description}>
          {t("AloneOrGroup", "A lane works fine on its own. A lane group is only needed when the same lane comes in several versions that players switch between.")}
        </p>
      </Card>

      <Card title={t("StartLane", "Start a new lane")} description={t("StartLaneHint", "Copies an existing net section, adds Road Builder components to the copy and selects it, ready to set up.")}>
        <Row label={t("SourceSection", "Section to copy")} hint={t("SourceSectionHint", "Pick the section closest to the lane you want to make.")}>
          <Picker value={source} placeholder={t("PickSection", "Pick a net section…")} items={sections.map((x) => ({ value: x, label: x }))} onSelect={(v) => setSource(v || undefined)} />
        </Row>
        <div className={styles.inlineButtons}>
          <Button variant="flat" className={styles.smallButton} disabled={!source} onSelect={source ? () => createLaneSetupFromSection(source, "lane") : undefined}>
            {t("CreateLane", "Create lane")}
          </Button>
          <Button variant="flat" className={styles.smallButton} disabled={!source} onSelect={source ? () => createLaneSetupFromSection(source, "edge") : undefined}>
            {t("CreateEdgeLane", "Create edge lane")}
          </Button>
        </div>
      </Card>

      <Card
        title={t("StartGroup", "Start a lane group")}
        description={t("StartGroupHint", "Creates an empty lane group and selects it. Add its options, then add variants from the group's setup.")}
      >
        <Row
          label={t("GroupName", "Group name")}
          hint={t("GroupNameHint", "Required. Players' roads save lanes by their group's name, so pick one you won't need to change.")}
        >
          <TextField live value={groupName} placeholder={t("GroupNamePlaceholder", "e.g. Tram Platform")} onCommit={setGroupName} />
          {nameTaken && <span className={styles.fieldError}>{t("GroupNameTaken", "A lane group with this name already exists.")}</span>}
        </Row>
        <Button variant="flat" className={styles.smallButton} disabled={!canCreateGroup} onSelect={canCreateGroup ? () => createLaneSetupGroup(trimmedName) : undefined}>
          {t("CreateGroup", "Create lane group")}
        </Button>
      </Card>

      <Card title={t("EditExisting", "Edit an existing one")}>
        <p className={styles.description}>
          {t("EditExistingText", "Select a net section or lane group in the inspector, and this panel switches to its Road Builder setup.")}
        </p>
        <p className={styles.description}>
          {t("SavingText", "Save new prefabs with the asset editor as usual. Players need Road Builder installed to load them, so set it as a required item when publishing.")}
        </p>
      </Card>
    </>
  );
};
