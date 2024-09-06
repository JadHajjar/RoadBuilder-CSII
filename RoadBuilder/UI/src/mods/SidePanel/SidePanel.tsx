import { Button, FOCUS_DISABLED, Scrollable, Tooltip } from "cs2/ui";
import { LaneListItem } from "../Components/LaneListItem/LaneListItem";
import styles from "./SidePanel.module.scss";
import { useValue } from "cs2/api";
import {
  allNetSections$,
  allRoadConfigurations$,
  fpsMeterLevel$,
  manageRoads,
  roadBuilderToolMode$,
  roadListView$,
  setRoadListView,
  setRoadsSearchBinder,
  setSearchBinder,
} from "mods/bindings";
import { useEffect, useState } from "react";
import { useLocalization } from "cs2/l10n";
import { SearchTextBox } from "mods/Components/SearchTextBox/SearchTextBox";
import { RoadConfigListItem } from "mods/Components/RoadConfigListItem/RoadConfigListItem";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import classNames from "classnames";
import { LaneListGroup } from "mods/Components/LaneListItem/LaneListGroup";
import { GetCategoryIcon, GetCategoryName, RoadCategory } from "domain/RoadCategory";

export const SidePanel = (props: { editor: boolean }) => {
  const { translate } = useLocalization();
  const toolMode = useValue(roadBuilderToolMode$);
  const roadListView = useValue(roadListView$);
  const roadConfigurations = useValue(allRoadConfigurations$);
  const netSections = useValue(allNetSections$);
  const fpsMeterLevel = useValue(fpsMeterLevel$);
  let [selectedCategory, setSelectedCategory] = useState<string | RoadCategory | undefined>(undefined);
  let [searchQuery, setSearchQuery] = useState<string>("");
  let items: JSX.Element[];

  function setAndBindSearch(query: string) {
    setSearchQuery(query);

    if (roadListView || toolMode == RoadBuilderToolModeEnum.Picker) setRoadsSearchBinder(query);
    else setSearchBinder(query);
  }

  useEffect(() => {
    // when the road list view changes value
    setAndBindSearch("");
  }, [roadListView]);

  //if (toolMode == RoadBuilderToolModeEnum.Picker && searchQuery === "" && roadConfigurations.length === 0) return <></>;

  if (roadListView || toolMode == RoadBuilderToolModeEnum.Picker) {
    items = roadConfigurations
      .filter((val, idx) => (selectedCategory == undefined || selectedCategory == val.Category) && !val.IsNotInPlayset)
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
    <div
      className={classNames(
        styles.panel,
        props.editor ? styles.editor : styles.game,
        styles["fpsLevel" + fpsMeterLevel],
        toolMode == RoadBuilderToolModeEnum.Picker && styles.expanded
      )}
    >
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
        {(roadListView || toolMode == RoadBuilderToolModeEnum.Picker) && (
          <div className={styles.categories}>
            <Tooltip tooltip={translate("RoadBuilder.AllRoads")}>
              <Button className={selectedCategory == undefined && styles.selected} variant="flat" onSelect={() => setSelectedCategory(undefined)}>
                <img src="Media/Tools/Snap Options/All.svg" />
              </Button>
            </Tooltip>
            {Object.values(RoadCategory)
              .filter((x) => GetCategoryIcon(x as RoadCategory) != undefined && roadConfigurations.some((r) => r.Category === x))
              .map((x) => (
                <Tooltip tooltip={translate(GetCategoryName(x as RoadCategory))}>
                  <Button className={selectedCategory == x && styles.selected} variant="flat" onSelect={() => setSelectedCategory(x)}>
                    <img src={GetCategoryIcon(x as RoadCategory)} />
                  </Button>
                </Tooltip>
              ))}
          </div>
        )}
      </div>
      <Scrollable className={styles.list} vertical smooth trackVisibility="scrollable">
        {items}
      </Scrollable>
      {toolMode == RoadBuilderToolModeEnum.Picker && (
        <div className={styles.manageRoadsButton}>
          <Button onSelect={manageRoads} variant="flat" focusKey={FOCUS_DISABLED}>
            <img style={{ maskImage: "url(coui://roadbuildericons/RB_WhiteWrench.svg)" }} />
            {translate("RoadBuilder.ManageRoads", "Manage Your Roads")}
          </Button>
        </div>
      )}
    </div>
  );
};
