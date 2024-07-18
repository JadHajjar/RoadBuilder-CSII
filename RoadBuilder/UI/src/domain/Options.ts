export interface OptionSection {
  id: number;
  name: string;
  options: OptionItem[];
}

export interface OptionItem {
  id: number;
  name: string;
  icon: string;
  selected: boolean;
  isValue: boolean;
  disabled: boolean;
  value: string;
}
