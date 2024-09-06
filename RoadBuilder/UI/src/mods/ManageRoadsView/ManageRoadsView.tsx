import { Button, Scrollable, Tooltip } from "cs2/ui";
import { LaneListItem } from "../Components/LaneListItem/LaneListItem";
import styles from "./ManageRoadsView.module.scss";
import { useValue } from "cs2/api";
import {
  allRoadConfigurations$,
  DiscoverErrorLoading$,
  DiscoverItems$,
  DiscoverLoading$,
  managedRoad$,
  RestrictPlayset$,
  setDiscoverSearchBinder,
  setDiscoverSorting,
  setManagementRoad,
  setManagementSearchBinder,
  setManagementSetCategory,
  toggleTool,
} from "mods/bindings";
import { useEffect, useState } from "react";
import { useLocalization } from "cs2/l10n";
import { SearchTextBox } from "mods/Components/SearchTextBox/SearchTextBox";
import classNames from "classnames";
import { DiscoverRoadConfigListItem } from "mods/Components/RoadConfigListItem/DiscoverRoadConfigListItem";
import { GetCategoryIcon, GetCategoryName, RoadCategory } from "domain/RoadCategory";
import { ManageRoadConfigListItem } from "mods/Components/RoadConfigListItem/ManageRoadConfigListItem";
import { ManageRoadPanel } from "./ManageRoadPanel";
import { Pagination } from "./Pagination";
import { CustomDropdown } from "mods/Components/DropDown/DropDown";

export const ManageRoadsView = (props: { editor: boolean }) => {
  const { translate } = useLocalization();
  const DiscoverLoading = useValue(DiscoverLoading$);
  const DiscoverErrorLoading = useValue(DiscoverErrorLoading$);
  const DiscoverItems = useValue(DiscoverItems$);
  const RestrictPlayset = useValue(RestrictPlayset$);
  const roadConfigurations = useValue(allRoadConfigurations$);
  const managedRoad = useValue(managedRoad$);
  let [searchQuery, setSearchQuery] = useState<string>("");
  let [discoverView, setDiscoverView] = useState<boolean>(false);
  let [showAllPlaysets, setShowAllPlaysets] = useState<boolean>(false);
  let [hideLocked, setHideLocked] = useState<boolean>(false);
  let [usedFilter, setUsedFilter] = useState<boolean | undefined>(undefined);
  let [sorting, setSorting] = useState<number>(0);
  let [selectedCategory, setSelectedCategory] = useState<string | RoadCategory | undefined>(undefined);
  let items: JSX.Element;

  function setAndBindSearch(query: string) {
    setSearchQuery(query);

    if (discoverView) setDiscoverSearchBinder(query);
    else setManagementSearchBinder(query);
  }

  function setAndBindCategory(cat: RoadCategory | undefined) {
    setSelectedCategory(cat);

    if (discoverView) setManagementSetCategory(cat ?? -1);
  }

  function setAndBindSorting(value: number) {
    setSorting(value);
    if (discoverView) setDiscoverSorting(value);
  }

  if (!managedRoad && roadConfigurations.length > 0) setManagementRoad(roadConfigurations[0].ID);

  useEffect(() => {
    setAndBindSearch("");
    setAndBindCategory(undefined);
    setAndBindSorting(0);
    setUsedFilter(undefined);
  }, [discoverView]);

  if (discoverView) {
    {
      items = (
        <div className={styles.browseContainer}>
          {DiscoverLoading && (
            <div className={styles.loader}>
              <img src="coui://roadbuildericons/RB_Loader.svg" />
            </div>
          )}

          {!DiscoverLoading && (DiscoverErrorLoading || DiscoverItems.length === 0) && (
            <div className={styles.loader}>
              <span className={styles.noResults}>
                <img src={DiscoverErrorLoading ? "Media/Misc/Warning.svg" : "coui://ui-mods/images/RB_Magnifier.svg"} />
                {translate(DiscoverErrorLoading ? "RoadBuilder.ErrorLoading" : "RoadBuilder.NoResults")}
              </span>
            </div>
          )}

          {!DiscoverLoading && !DiscoverErrorLoading && (
            <>
              <Scrollable className={styles.list} vertical smooth trackVisibility="scrollable">
                {DiscoverItems.map((val, idx) => (
                  <DiscoverRoadConfigListItem key={idx} road={val} />
                ))}
              </Scrollable>
              <Pagination />
            </>
          )}
        </div>
      );
    }
  } else {
    items = (
      <div className={styles.localContainer}>
        <Scrollable className={styles.list} vertical smooth trackVisibility="scrollable">
          {roadConfigurations
            .filter(
              (val, idx) =>
                (selectedCategory == undefined || selectedCategory == val.Category) &&
                (usedFilter === undefined || usedFilter == val.Used) &&
                (!val.IsNotInPlayset || showAllPlaysets) &&
                (!val.Locked || !hideLocked)
            )
            .filter((val, idx) => searchQuery == undefined || searchQuery == "" || val.Name.toLowerCase().indexOf(searchQuery.toLowerCase()) >= 0)
            .map((val, idx) => (
              <ManageRoadConfigListItem key={idx} road={val} selectRoad={(r) => setManagementRoad(r.ID)} selected={managedRoad?.ID === val.ID} />
            ))}
        </Scrollable>
        <div className={styles.managePanel}>{managedRoad && <ManageRoadPanel road={managedRoad} />}</div>
      </div>
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
        <div className={styles.topBar}>
          <div className={styles.categories}>
            <Button className={selectedCategory == undefined && styles.selected} variant="flat" onSelect={() => setAndBindCategory(undefined)}>
              <img src="Media/Tools/Snap Options/All.svg" />
              <span>{translate("RoadBuilder.AllRoads")}</span>
            </Button>
            {Object.values(RoadCategory)
              .filter((x) => GetCategoryIcon(x as RoadCategory) != undefined && (discoverView || roadConfigurations.some((r) => r.Category === x)))
              .map((x) => (
                <Button className={selectedCategory == x && styles.selected} variant="flat" onSelect={() => setAndBindCategory(x as RoadCategory)}>
                  <img src={GetCategoryIcon(x as RoadCategory)} />
                  <span>{translate(GetCategoryName(x as RoadCategory))}</span>
                </Button>
              ))}
          </div>

          <div className={styles.filters}>
            {!discoverView && RestrictPlayset && (
              <Tooltip tooltip={translate("RoadBuilder.ShowAllPlaysets")}>
                <Button variant="flat" selected={showAllPlaysets} onSelect={() => setShowAllPlaysets(!showAllPlaysets)}>
                  <img style={{ maskImage: "url(coui://roadbuildericons/RB_Playset.svg)" }} />
                </Button>
              </Tooltip>
            )}

            {!discoverView && roadConfigurations.some((x) => x.Locked) && (
              <Tooltip tooltip={translate("RoadBuilder.HideLockedRoads")}>
                <Button variant="flat" selected={hideLocked} onSelect={() => setHideLocked(!hideLocked)}>
                  <img style={{ maskImage: "url(coui://roadbuildericons/RB_NoLock.svg)" }} />
                </Button>
              </Tooltip>
            )}

            {!discoverView && (
              <Tooltip tooltip={translate("RoadBuilder.ShowHidePlaced")}>
                <Button
                  variant="flat"
                  selected={usedFilter != undefined}
                  style={{
                    backgroundColor:
                      usedFilter === false
                        ? "var(--animation-curve-active-n1-stroke)"
                        : usedFilter
                        ? "var(--animation-curve-active-n2-stroke)"
                        : undefined,
                  }}
                  onSelect={() => setUsedFilter(usedFilter == undefined ? true : usedFilter ? false : undefined)}
                >
                  <img
                    style={{
                      maskImage:
                        usedFilter === false ? "url(coui://roadbuildericons/RB_NoLocation.svg)" : "url(coui://roadbuildericons/RB_Location.svg)",
                    }}
                  />
                </Button>
              </Tooltip>
            )}

            {discoverView && (
              <div className={styles.sortingDropdown}>
                <CustomDropdown<number>
                  SelectedItem={sorting}
                  Items={[0, 1, 2, 3, 4, 5]}
                  ToString={(x) => translate(`RoadBuilder.DiscoverSorting[${x}]`)}
                  Icon="coui://roadbuildericons/RB_Sort.svg"
                  OnItemSelected={setAndBindSorting}
                ></CustomDropdown>
              </div>
            )}

            <div className={styles.searchBar}>
              <SearchTextBox value={searchQuery} onChange={setAndBindSearch} />
            </div>
          </div>
        </div>

        <Button
          className={styles.closeButton}
          variant="flat"
          onSelect={() => {
            toggleTool();
            toggleTool();
          }}
        >
          <img style={{ maskImage: "url(Media/Glyphs/Close.svg)" }} />
        </Button>
      </div>
      {items}
    </div>
  );
};
