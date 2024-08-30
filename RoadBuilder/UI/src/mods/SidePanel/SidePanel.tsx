import { Scrollable } from "cs2/ui";
import { LaneListItem } from "../Components/LaneListItem/LaneListItem";
import styles from "./SidePanel.module.scss";
import { useValue } from "cs2/api";
import { allNetSections$, allRoadConfigurations$, roadBuilderToolMode$, roadListView$, setRoadListView } from "mods/bindings";
import { useEffect, useState } from "react";
import { useLocalization } from "cs2/l10n";
import { SearchTextBox } from "mods/Components/SearchTextBox/SearchTextBox";
import { RoadConfigListItem } from "mods/Components/RoadConfigListItem/RoadConfigListItem";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import classNames from "classnames";

export const SidePanel = (props: { editor: boolean }) => {
  const { translate } = useLocalization();
  const toolMode = useValue(roadBuilderToolMode$);
  const roadListView = useValue(roadListView$);
  const roadConfigurations = useValue(allRoadConfigurations$);
  const netSections = useValue(allNetSections$);
  let [searchQuery, setSearchQuery] = useState<string>("");
  let items: JSX.Element[];

  useEffect(() => {
    // when the road list view changes value
    setSearchQuery("");
  }, [roadListView]);

  if (toolMode == RoadBuilderToolModeEnum.Picker && roadConfigurations.length === 0) return <></>;

  if (roadListView || toolMode == RoadBuilderToolModeEnum.Picker) {
    items = roadConfigurations
      .filter((val, idx) => val.Name)
      .filter((val, idx) => searchQuery == undefined || searchQuery == "" || val.Name.toLowerCase().indexOf(searchQuery.toLowerCase()) >= 0)
      .map((val, idx) => <RoadConfigListItem key={idx} road={val} />);
  } else {
    items = netSections
      .filter((val, idx) => val.PrefabName)
      .filter((val, idx) => searchQuery == undefined || searchQuery == "" || val.DisplayName.toLowerCase().indexOf(searchQuery.toLowerCase()) >= 0)
      .map((val, idx) => <LaneListItem key={idx} netSection={val} />);
  }

  return (
    <div className={classNames(styles.panel, props.editor ? styles.editor : styles.game)}>
      <div className={styles.header}>
        {toolMode == RoadBuilderToolModeEnum.Picker && (
          <div className={styles.subHeader}>
            <div className={styles.title}>{translate("RoadBuilder.CreatedRoads")}</div>
            <div className={styles.roadCount}>{translate("RoadBuilder.RoadCount")?.replace("{0}", roadConfigurations.length.toString())}</div>
          </div>
        )}
        {toolMode != RoadBuilderToolModeEnum.Picker && (
          <div className={styles.mode}>
            <div
              className={!roadListView && styles.selected}
              onClick={() => {
                setRoadListView(false);
                setSearchQuery("");
              }}
            >
              {translate("RoadBuilder.AvailableLanes")}
            </div>
            <div
              className={roadListView && styles.selected}
              onClick={() => {
                setRoadListView(true);
                setSearchQuery("");
              }}
            >
              {translate("RoadBuilder.CreatedRoads")}
            </div>
          </div>
        )}
        <div style={{ marginTop: "6rem" }}>
          <SearchTextBox value={searchQuery} onChange={setSearchQuery} />
        </div>
      </div>
      <Scrollable className={styles.list} vertical smooth trackVisibility="scrollable">
        {items}
      </Scrollable>
    </div>
  );
};
