import { Button, Tooltip } from "cs2/ui";
import styles from "./ManageRoadPanel.module.scss";
import { NetSectionItem } from "domain/NetSectionItem";
import { CSSProperties, MouseEventHandler, forwardRef, useContext, useEffect, useRef, useState } from "react";
import classNames from "classnames";
import { DragContext } from "mods/Contexts/DragContext";
import { MouseButtons } from "mods/util";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { RoadConfiguration } from "domain/RoadConfiguration";
import {
  deleteRoad,
  editRoad,
  findRoad,
  activateRoad,
  getRoadId$,
  setRoadName,
  getRoadName$,
  RestrictPlayset$,
  managedRoadOptionClicked,
  managedRoadOptions$,
  setManagedRoadName,
} from "mods/bindings";
import { useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { GetCategoryIcon, GetCategoryName, RoadCategory } from "domain/RoadCategory";
import { OptionsPanelComponent } from "mods/Components/OptionsPanel/OptionsPanel";

export const ManageRoadPanel = ({ road }: { road: RoadConfiguration }) => {
  const { translate } = useLocalization();
  const managedRoadOptions = useValue(managedRoadOptions$);
  const nameRef = useRef(null);
  const [showNameTextbox, setTextboxVisibility] = useState<boolean>(false);

  let [newRoadName, setNewRoadName] = useState(road.Name);

  let onFinishEditRoadName = () => {
    setManagedRoadName(newRoadName);
    setTextboxVisibility(false);
  };

  let onStartEditRoadName = () => {
    setNewRoadName(road.Name);
  };

  useEffect(() => {
    if (nameRef === null || nameRef.current === null) return;
    console.log((nameRef.current as any).firstChild.firstChild);
    (nameRef.current as any).firstChild.firstChild.focus();
    (nameRef.current as any).firstChild.firstChild.select();
  }, [showNameTextbox]);

  return (
    <>
      <img className={styles.thumbnail} src={road.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"} />
      {showNameTextbox ? (
        <div className={styles.nameBox} ref={nameRef}>
          <VanillaComponentResolver.instance.EllipsisTextInput
            onChange={({ target }) => setNewRoadName(target.value)}
            placeholder={translate("RoadBuilder.RoadName", "Road Name")!}
            value={newRoadName}
            className={styles.textInput}
            onBlur={onFinishEditRoadName}
            onFocus={onStartEditRoadName}
            maxLength={250}
          />
        </div>
      ) : (
        <div className={styles.name} onClick={() => setTextboxVisibility(true)}>
          <p>{road.Name}</p>
        </div>
      )}
      <div className={styles.roadType}>
        <img src={GetCategoryIcon(road.Category)} />
        <span>{translate(GetCategoryName(road.Category))}</span>
      </div>

      <div className={styles.divider} />

      <div className={styles.options}>
        <OptionsPanelComponent OnChange={managedRoadOptionClicked} options={managedRoadOptions} />
      </div>

      <div className={styles.divider} />

      <div className={styles.buttons}>
        <Tooltip tooltip={translate("RoadBuilder.PlaceTooltip")}>
          <Button variant="flat" onSelect={() => activateRoad(road.ID)}>
            <img style={{ maskImage: "url(coui://roadbuildericons/RB_PlaceMore.svg)", width: "24rem", height: "24rem", marginRight: "6rem" }} />{" "}
            {translate("RoadBuilder.Place")}
          </Button>
        </Tooltip>
        <Tooltip tooltip={translate("RoadBuilder.EditTooltip")}>
          <Button variant="flat" onSelect={() => editRoad(road.ID)}>
            <img style={{ maskImage: "url(coui://roadbuildericons/RB_Edit.svg)" }} /> {translate("RoadBuilder.Edit")}
          </Button>
        </Tooltip>
        <Tooltip tooltip={translate("RoadBuilder.FindTooltip")}>
          <Button disabled={!road.Used} variant="flat" onSelect={() => findRoad(road.ID)} className={!road.Used && styles.disabled}>
            <img style={{ maskImage: "url(coui://roadbuildericons/RB_Location.svg)" }} /> {translate("RoadBuilder.Find")}
          </Button>
        </Tooltip>
        <Tooltip tooltip={translate("RoadBuilder.DeleteTooltip")}>
          <Button variant="flat" onSelect={() => deleteRoad([road.ID])} className={styles.danger}>
            <img style={{ maskImage: "url(coui://roadbuildericons/RB_Trash.svg)" }} /> {translate("RoadBuilder.Delete")}
          </Button>
        </Tooltip>
      </div>
    </>
  );
};
