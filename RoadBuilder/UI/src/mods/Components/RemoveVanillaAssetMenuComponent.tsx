import { bindValue, useValue } from "cs2/api";
import { ModuleRegistryExtend } from "cs2/modding";
import { roadBuilderToolMode$ } from "mods/bindings";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";

export const RemoveVanillaAssetMenuComponent: ModuleRegistryExtend = (Component) => {
  return (props) => {
    const { children, ...otherProps } = props || {};

    const roadBuilderToolMode = useValue(roadBuilderToolMode$);

    if (roadBuilderToolMode == RoadBuilderToolModeEnum.Editing || roadBuilderToolMode == RoadBuilderToolModeEnum.EditingSingle) {
      return <></>;
    }

    return <Component {...otherProps}>{children}</Component>;
  };
};
