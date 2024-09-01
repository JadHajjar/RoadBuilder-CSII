import { Button, Scrollable } from "cs2/ui";
import { LaneListItem } from "../Components/LaneListItem/LaneListItem";
import styles from "./ManageRoadsView.module.scss";
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
import { DiscoverRoadConfigListItem } from "mods/Components/RoadConfigListItem/DiscoverRoadConfigListItem";
import { GetCategoryIcon, GetCategoryName, RoadCategory } from "domain/RoadCategory";

export const ManageRoadsView = (props: { editor: boolean }) => {
  const { translate } = useLocalization();
  const roadConfigurations = useValue(allRoadConfigurations$);
  let [searchQuery, setSearchQuery] = useState<string>("");
  let [discoverView, setDiscoverView] = useState<boolean>(false);
  let [selectedCategory, setSelectedCategory] = useState<string | RoadCategory | undefined>(undefined);
  let items: JSX.Element;

  function setAndBindSearch(query: string) {
    setSearchQuery(query);
    setSearchBinder(query);
  }

  useEffect(() => {
    // when the road list view changes value
    setAndBindSearch("");
  }, [discoverView]);

  if (discoverView) {
    items = (
      <div className={styles.browseContainer}>
        <Scrollable className={styles.list} vertical smooth trackVisibility="scrollable">
          {roadConfigurations
            .filter((val, idx) => val.Name)
            .filter((val, idx) => searchQuery == undefined || searchQuery == "" || val.Name.toLowerCase().indexOf(searchQuery.toLowerCase()) >= 0)
            .map((val, idx) => (
              <DiscoverRoadConfigListItem key={idx} road={val} />
            ))}
        </Scrollable>

        <div className={styles.paging}>1</div>
      </div>
    );
  } else {
    items = (
      <Scrollable className={styles.list} vertical smooth trackVisibility="scrollable">
        {roadConfigurations
          .filter((val, idx) => val.Name)
          .filter((val, idx) => searchQuery == undefined || searchQuery == "" || val.Name.toLowerCase().indexOf(searchQuery.toLowerCase()) >= 0)
          .map((val, idx) => (
            <RoadConfigListItem key={idx} road={val} />
          ))}
      </Scrollable>
    );
  }

  return (
    <div className={classNames(styles.panel, props.editor ? styles.editor : styles.game)}>
      <div className={styles.header}>
        <div className={styles.mode}>
          <div className={!discoverView && styles.selected} onClick={() => setDiscoverView(false)}>
            <img style={{ maskImage: "url(coui://roadbuildericons/RB_User.svg)" }} />
            <span>{translate("RoadBuilder.MyRoads")}</span>
          </div>
          <div className={discoverView && styles.selected} onClick={() => setDiscoverView(true)}>
            <img style={{ maskImage: "url(coui://roadbuildericons/RB_Compass.svg)" }} />
            <span>{translate("RoadBuilder.Discover")}</span>
          </div>
        </div>
        <div className={styles.filters}>
          <div className={styles.searchBar}>
            <SearchTextBox value={searchQuery} onChange={setAndBindSearch} />
          </div>
          <div className={styles.categories}>
            <Button className={selectedCategory == undefined && styles.selected} variant="flat" onSelect={() => setSelectedCategory(undefined)}>
              <img src="Media/Tools/Snap Options/All.svg" />
              <span>{translate("RoadBuilder.AllRoads")}</span>
            </Button>
            {Object.values(RoadCategory)
              .filter((x) => GetCategoryIcon(x as RoadCategory) != undefined)
              .map((x) => (
                <Button className={selectedCategory == x && styles.selected} variant="flat" onSelect={() => setSelectedCategory(x)}>
                  <img src={GetCategoryIcon(x as RoadCategory)} />
                  <span>{translate(GetCategoryName(x as RoadCategory))}</span>
                </Button>
              ))}
          </div>
        </div>
      </div>
      {items}
    </div>
  );
};
