import { useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import { Tooltip } from "cs2/ui";
import { getModule, ModuleRegistryExtend } from "cs2/modding";
import { Children, cloneElement, isValidElement, ReactNode } from "react";
import classNames from "classnames";
import laneSetupIcon from "images/RB_LaneSetup.svg";
import { useSetupTranslate } from "./Fields";
import { laneSetupOpen$, laneSetupState$, setLaneSetupOpen } from "./laneSetupBindings";
import styles from "./LaneSetup.module.scss";

const IconButton = getModule("game-ui/common/input/button/icon-button.tsx", "IconButton");

const BottomBarTheme: Theme | any = getModule("game-ui/editor/components/bottom-bar/bottom-bar.module.scss", "classes");

const BottomBarButtonTheme: Theme | any = getModule(
  "game-ui/editor/components/bottom-bar/bottom-bar-floating-hint-button.module.scss",
  "classes"
);

const LaneSetupBottomBarButton = () => {
  const t = useSetupTranslate();
  const state = useValue(laneSetupState$);
  const open = useValue(laneSetupOpen$);
  // Always usable in the editor, without a selection the panel opens to its getting started view
  const available = state.InEditor;

  const tooltip = !state.Available
    ? t("GettingStartedTooltip", "Road Builder custom lanes: getting started")
    : state.Role === "none"
    ? t("SetUpForRoadBuilder", "Set up for Road Builder")
    : t("EditRoadBuilderSetup", "Road Builder setup");

  return (
    <Tooltip tooltip={tooltip}>
      <div className={styles.bottomBarButton}>
        <IconButton
          tinted
          src={laneSetupIcon}
          theme={BottomBarButtonTheme}
          disabled={!available}
          className={classNames(BottomBarTheme.button, open && available && BottomBarTheme.buttonHighlight, !available && styles.bottomBarButtonDisabled)}
          onSelect={available ? () => setLaneSetupOpen(!open) : undefined}
        />
      </div>
    </Tooltip>
  );
};

const insertButton = (node: ReactNode): ReactNode => {
  if (!isValidElement(node)) return node;

  const props = node.props as any;

  if (props.className === BottomBarTheme.inspectorModes) {
    return cloneElement(node, undefined, ...Children.toArray(props.children), <LaneSetupBottomBarButton key="roadbuilder-lane-setup" />);
  }

  if (props.children === undefined) return node;

  if (Array.isArray(props.children)) {
    const children = props.children.map(insertButton);

    return children.some((x: ReactNode, i: number) => x !== props.children[i]) ? cloneElement(node, undefined, ...children) : node;
  }

  const child = insertButton(props.children);

  return child !== props.children ? cloneElement(node, undefined, child) : node;
};

export const LaneSetupBottomBarExtension: ModuleRegistryExtend = (Component: any) => {
  return (props: any) => insertButton(Component(props)) as JSX.Element;
};
