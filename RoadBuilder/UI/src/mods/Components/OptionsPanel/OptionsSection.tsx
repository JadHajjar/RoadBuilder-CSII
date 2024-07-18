import styles from "./OptionsPanel.module.scss";
import { PropsWithChildren } from "react";

type _Props = {
  name: string;
};

export const OptionsSection = (props: PropsWithChildren<_Props>) => {
  return (
    <div className={styles.optionRow}>
      <div className={styles.optionSection}>
        <div className={styles.optionLabel}>{props.name}</div>
        <div className={styles.optionContent}>{props.children}</div>
      </div>
    </div>
  );
};
