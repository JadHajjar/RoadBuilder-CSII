import { Scrollable } from "cs2/ui";
import { LaneListItem } from "../Components/LaneListItem/LaneListItem";
import styles from "./SidePanel.module.scss";
import { useValue } from "cs2/api";
import {
  allNetSections$,
  allRoadConfigurations$,
  fpsMeterLevel$,
  roadBuilderToolMode$,
  roadListView$,
  setRoadListView,
  setSearchBinder,
} from "mods/bindings";
import { useEffect, useState } from "react";
import { useLocalization } from "cs2/l10n";
import { SearchTextBox } from "mods/Components/SearchTextBox/SearchTextBox";
import { RoadConfigListItem } from "mods/Components/RoadConfigListItem/RoadConfigListItem";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import classNames from "classnames";
import { LaneListGroup } from "mods/Components/LaneListItem/LaneListGroup";

export const SidePanel = (props: { editor: boolean }) => {
  const { translate } = useLocalization();
  const toolMode = useValue(roadBuilderToolMode$);
  const roadListView = useValue(roadListView$);
  const roadConfigurations = useValue(allRoadConfigurations$);
  const netSections = useValue(allNetSections$);
  const fpsMeterLevel = useValue(fpsMeterLevel$);
  let [searchQuery, setSearchQuery] = useState<string>("");
  let items: JSX.Element[];

  function setAndBindSearch(query: string) {
    setSearchQuery(query);
    setSearchBinder(query);
    console.log(query);
  }

  useEffect(() => {
    // when the road list view changes value
    setAndBindSearch("");
  }, [roadListView]);

  if (toolMode == RoadBuilderToolModeEnum.Picker && roadConfigurations.length === 0) return <></>;

  if (roadListView || toolMode == RoadBuilderToolModeEnum.Picker) {
    items = roadConfigurations
      .filter((val, idx) => val.Name)
      .filter((val, idx) => searchQuery == undefined || searchQuery == "" || val.Name.toLowerCase().indexOf(searchQuery.toLowerCase()) >= 0)
      .map((val, idx) => <RoadConfigListItem key={idx} road={val} />);
  } else {
    const small = netSections.map((x) => x.Sections.length).reduce((x, s) => x + s, 0) >= 15;
    items = netSections
      .sort((a, b) => (a.Type > b.Type ? 1 : -1))
      .map((grp) => (
        <LaneListGroup type={grp.Type} small={small}>
          {grp.Sections.map((val, idx) => (
            <LaneListItem key={idx} netSection={val} small={small} />
          ))}
        </LaneListGroup>
      ));
  }

  return (
    <div className={classNames(styles.panel, props.editor ? styles.editor : styles.game, styles["fpsLevel" + fpsMeterLevel])}>
      <div className={styles.header}>
        {toolMode == RoadBuilderToolModeEnum.Picker && (
          <div className={styles.subHeader}>
            <div className={styles.title}>{translate("RoadBuilder.CreatedRoads")}</div>
            <div className={styles.roadCount}>{translate("RoadBuilder.RoadCount")?.replace("{0}", roadConfigurations.length.toString())}</div>
          </div>
        )}
        {toolMode != RoadBuilderToolModeEnum.Picker && (
          <div className={styles.mode}>
            <div className={!roadListView && styles.selected} onClick={() => setRoadListView(false)}>
              {translate("RoadBuilder.AvailableLanes")}
            </div>
            <div className={roadListView && styles.selected} onClick={() => setRoadListView(true)}>
              {translate("RoadBuilder.CreatedRoads")}
            </div>
          </div>
        )}
        <div style={{ marginTop: "6rem" }}>
          <SearchTextBox value={searchQuery} onChange={setAndBindSearch} />
        </div>
      </div>
      <Scrollable className={styles.list} vertical smooth trackVisibility="scrollable">
        {items}
      </Scrollable>
    </div>
  );
};
