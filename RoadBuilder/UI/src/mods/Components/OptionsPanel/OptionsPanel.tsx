import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import mod from "mod.json";
import { getModule } from "cs2/modding";
import styles from "./OptionsPanel.module.scss";
import { OptionSection } from "domain/RoadProperties";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { Tooltip } from "cs2/ui";

type _Props = {
  options: OptionSection[];
  Index: number;
};

export const OptionsPanelComponent = (props: _Props) => {
  return (
    <>
      {props.options.map((section) => (
        <div className={styles.optionRow}>
          <div className={styles.optionSection}>
            <div className={styles.optionLabel}>{section.name}</div>
            <div className={styles.optionContent}>
              {section.options.map((option) =>
                option.isValue ? (
                  <>
                    <VanillaComponentResolver.instance.ToolButton
                      onSelect={() => trigger(mod.id, "OptionClicked", props.Index, section.id, option.id, -1)}
                      src="Media/Glyphs/ThickStrokeArrowDown.svg"
                      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                      className={
                        VanillaComponentResolver.instance.toolButtonTheme.button +
                        " " +
                        VanillaComponentResolver.instance.mouseToolOptionsTheme.startButton
                      }
                    />

                    <div className={VanillaComponentResolver.instance.mouseToolOptionsTheme.numberField}>{option.value}</div>

                    <VanillaComponentResolver.instance.ToolButton
                      onSelect={() => trigger(mod.id, "OptionClicked", props.Index, section.id, option.id, 1)}
                      src="Media/Glyphs/ThickStrokeArrowUp.svg"
                      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                      className={
                        VanillaComponentResolver.instance.toolButtonTheme.button +
                        " " +
                        VanillaComponentResolver.instance.mouseToolOptionsTheme.endButton
                      }
                    />
                  </>
                ) : (
                  <VanillaComponentResolver.instance.ToolButton
                    selected={option.selected}
                    tooltip={option.name}
                    disabled={option.disabled}
                    onSelect={option.disabled ? undefined : () => trigger(mod.id, "OptionClicked", props.Index, section.id, option.id, 0)}
                    src={option.icon}
                    focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    className={VanillaComponentResolver.instance.toolButtonTheme.button + " " + styles.singleButton}
                  />
                )
              )}
            </div>
          </div>
        </div>
      ))}
    </>
  );
};
