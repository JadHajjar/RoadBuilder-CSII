import { bindValue, useValue } from "cs2/api";
import { ModuleRegistryExtend } from "cs2/modding";
import { roadBuilderToolMode$ } from "mods/bindings";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";

export const RemoveVanillaRightToolbar: ModuleRegistryExtend = (Component) => {
  return (props) => {
    const { children, ...otherProps } = props || {};

    const roadBuilderToolMode = useValue(roadBuilderToolMode$);

      if (roadBuilderToolMode != RoadBuilderToolModeEnum.None) {
      return <></>;
    }

    return <Component {...otherProps}>{children}</Component>;
  };
};
