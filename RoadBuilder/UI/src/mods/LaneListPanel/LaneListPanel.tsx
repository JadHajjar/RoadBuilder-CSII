import { Button, FOCUS_DISABLED, FocusKey, Scrollable } from "cs2/ui";
import { LaneListItem } from "../Components/LaneListItem/LaneListItem";
import styles from "./LaneListPanel.module.scss";
import { useValue } from "cs2/api";
import { allNetSections$, allRoadConfigurations$, roadBuilderToolMode$, roadListView$, setRoadListView } from "mods/bindings";
import { getModule } from "cs2/modding";
import { MutableRefObject, MouseEventHandler, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { Theme } from "cs2/bindings";
import { useLocalization } from "cs2/l10n";
import { SearchTextBox } from "mods/Components/SearchTextBox/SearchTextBox";
import { RoadConfigListItem } from "mods/Components/RoadConfigListItem/RoadConfigListItem";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";

export const LaneListPanel = () => {
  const { translate } = useLocalization();
  const toolMode = useValue(roadBuilderToolMode$);
  const roadListView = useValue(roadListView$);
  let [searchQuery, setSearchQuery] = useState<string>();
  let items: JSX.Element[];

  if (roadListView) {
    const roadConfigurations = useValue(allRoadConfigurations$);
    items = roadConfigurations
      .filter((val, idx) => val.Name)
      .filter((val, idx) => searchQuery == undefined || searchQuery == "" || val.Name.toLowerCase().indexOf(searchQuery.toLowerCase()) >= 0)
      .map((val, idx) => <RoadConfigListItem key={idx} road={val} />);
  } else {
    const allNetSections = useValue(allNetSections$);
    items = allNetSections
      .filter((val, idx) => val.PrefabName)
      .filter((val, idx) => searchQuery == undefined || searchQuery == "" || val.DisplayName.toLowerCase().indexOf(searchQuery.toLowerCase()) >= 0)
      .map((val, idx) => <LaneListItem key={idx} netSection={val} />);
  }

  return (
    <div className={styles.panel}>
      <div className={styles.header}>
        {toolMode == RoadBuilderToolModeEnum.Picker && <div className={styles.title}>Created Roads</div>}
        <div style={{ marginTop: "6rem" }}>
          <SearchTextBox onChange={setSearchQuery} />
        </div>
      </div>
      {toolMode != RoadBuilderToolModeEnum.Picker && (
        <div className={styles.mode}>
          <div
            className={!roadListView && styles.selected}
            onClick={() => {
              setRoadListView(false);
              setSearchQuery("");
            }}
          >
            Available Lanes
          </div>
          <div
            className={roadListView && styles.selected}
            onClick={() => {
              setRoadListView(true);
              setSearchQuery("");
            }}
          >
            Created Roads
          </div>
        </div>
      )}
      <Scrollable className={styles.list} vertical smooth trackVisibility="scrollable">
        {items}
      </Scrollable>
    </div>
  );
};
