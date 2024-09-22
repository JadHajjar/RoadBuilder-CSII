import { bindValue, trigger, useValue } from "cs2/api";
import { Theme } from "cs2/bindings";
import mod from "mod.json";
import { getModule } from "cs2/modding";
import styles from "./OptionsPanel.module.scss";
import { OptionSection } from "domain/Options";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { Button, Tooltip } from "cs2/ui";
import { useContext } from "react";
import classNames from "classnames";

type _Props = {
  options: OptionSection[];
  OnChange: (x: number, y: number, z: number) => void;
};

export const OptionsPanelComponent = (props: _Props) => {
  return (
    <>
      {props.options?.map((section) => (
        <div className={styles.optionRow}>
          <div className={styles.optionSection}>
            <span className={styles.optionLabel}>{section.name}</span>
            <div
              className={classNames(
                styles.optionContent,
                section.isToggle && styles.toggleOption,
                section.isButton && styles.buttonOption,
                section.isCheckbox && styles.checkboxOption
              )}
            >
              {section.isCheckbox ? (
                <VanillaComponentResolver.instance.ToolButton
                  selected={section.options[0].selected}
                  tooltip={section.options[0].name}
                  disabled={section.options[0].disabled}
                  onSelect={section.options[0].disabled ? undefined : () => props.OnChange(section.id, section.options[0].id, 0)}
                  src={section.options[0].selected ? "coui://roadbuildericons/RB_Checkmark.svg" : ""}
                  focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                  className={classNames(
                    VanillaComponentResolver.instance.toolButtonTheme.button,
                    styles.singleButton,
                    section.options[0].selected && styles.selected,
                    !section.options[0].selected && styles.unselected,
                    section.options[0].disabled && styles.disabled
                  )}
                />
              ) : section.isButton ? (
                <Button
                  disabled={section.options[0].disabled}
                  onSelect={section.options[0].disabled ? undefined : () => props.OnChange(section.id, section.options[0].id, 0)}
                  focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                  className={classNames(
                    VanillaComponentResolver.instance.toolButtonTheme.button,
                    styles.singleButton,
                    section.options[0].disabled && styles.disabled
                  )}
                >
                  <img style={{ maskImage: `url(${section.options[0].icon})` }} />
                  {section.options[0].name}
                </Button>
              ) : (
                section.options?.map((option) =>
                  option.isValue ? (
                    <>
                      <VanillaComponentResolver.instance.ToolButton
                        onSelect={() => props.OnChange(section.id, option.id, -1)}
                        src="Media/Glyphs/ThickStrokeArrowDown.svg"
                        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                        className={classNames(
                          VanillaComponentResolver.instance.toolButtonTheme.button,
                          VanillaComponentResolver.instance.mouseToolOptionsTheme.startButton
                        )}
                      />

                      <div className={classNames(VanillaComponentResolver.instance.mouseToolOptionsTheme.numberField, styles.numberField)}>
                        {option.value}
                      </div>

                      <VanillaComponentResolver.instance.ToolButton
                        onSelect={() => props.OnChange(section.id, option.id, 1)}
                        src="Media/Glyphs/ThickStrokeArrowUp.svg"
                        focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                        className={classNames(
                          VanillaComponentResolver.instance.toolButtonTheme.button,
                          VanillaComponentResolver.instance.mouseToolOptionsTheme.endButton
                        )}
                      />
                    </>
                  ) : (
                    <VanillaComponentResolver.instance.ToolButton
                      selected={option.selected && !option.disabled}
                      tooltip={option.name}
                      disabled={option.disabled}
                      onSelect={option.disabled ? undefined : () => props.OnChange(section.id, option.id, 0)}
                      src={option.icon}
                      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                      className={classNames(
                        VanillaComponentResolver.instance.toolButtonTheme.button,
                        styles.singleButton,
                        option.selected && !option.disabled && styles.selected,
                        option.disabled && styles.disabled
                      )}
                    />
                  )
                )
              )}
            </div>
          </div>
        </div>
      ))}
    </>
  );
};
