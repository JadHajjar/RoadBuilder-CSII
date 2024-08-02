import { Number2 } from "cs2/ui";
import { roadLanes$, setRoadLanes } from "./bindings";
import { useValue } from "cs2/api";
import { RoadLane } from "domain/RoadLane";

export const classNames = (classes: { [className in string]: boolean }, ...baseClasses: string[]) => {
  let re = baseClasses.join(" ");
  re += Object.entries(classes)
    .filter(([className, value]) => value)
    .map(([className, value]) => className)
    .join(" ");
  return re;
};

export const range = (i: number, n: number) => ({
  [Symbol.iterator]: () => ({
    next: () => ({ done: i > n, value: i++ }),
  }),
});

export const MouseButtons = {
  Secondary: 2,
  Primary: 0,
};

export const intersects = (rect1: DOMRect, rect2: DOMRect) => {
  return !(rect1.right < rect2.left || rect1.left > rect2.right || rect1.bottom < rect2.top || rect1.top > rect2.bottom);
};

export const duplicateAt = (arr: any[], idx: number) => {
  return [...arr.slice(0, idx), arr[idx], ...arr.slice(Math.min(idx, arr.length))];
};

export const removeAt = (arr: any[], idx: number) => {
  return [...arr.slice(0, idx), ...arr.slice(Math.min(idx + 1, arr.length))];
};
