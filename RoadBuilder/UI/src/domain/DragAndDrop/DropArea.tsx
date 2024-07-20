import { DragContext } from "mods/Contexts/DragContext";
import { ReactElement, RefObject, useContext, useEffect, useRef, useState } from "react"
import { DropResult } from "./types";

export type DropAreaOptions = {
    /**
     * Called when something is drag-and-dropped into the specified area.
     * @param result The object containing the result of the drag and drop operation     
     */
    onDrop: (result: DropResult) => void;


    /**
     * Called when the DnD item is over the element.
     * @param data the payload of the drag-and-drop operation     
     */
    onHover: (data: any) => void;

    /**
     * Filters out irrelevant drag-and-drop operations.
     * @param data The payload being checked against for the dnd operation
     * @returns True if the drag operation is relevant to the area
     */
    filter: (data: any) => boolean;
}

export interface DroppableAreaElement {
    addEventListener: (type: any, listener: any) => void;
    removeEventListener: (type: any, listener: any) => void;
};

/**
 * Used to 
 * @param options Options for the drop area
 * @returns a list with a reference for the drop area element, and the drop result object
 */
export const useDropArea = <T extends DroppableAreaElement>(options?: Partial<DropAreaOptions>) : [RefObject<T>, DropResult | undefined] => {    
    let dragContext = useContext(DragContext);
    let [isHovered, setHovered] = useState(false);
    let [result, setResult] = useState<DropResult>();
    let areaRef = useRef<T>(null);    
    let config : DropAreaOptions = {
        onDrop: () => {},
        filter: () => true,
        onHover: () => {},
        ...options
    }
    
    let onMouseEnter = () => {
        setHovered(true);
    }
    let onMouseExit = () => {
        setHovered(false);
    }

    // Setup the callbacks when the element reference is set
    useEffect(() => {
        if (areaRef.current) {
            areaRef.current.addEventListener("mouseenter", onMouseEnter);
            areaRef.current.addEventListener("mouseleave", onMouseExit);
        }
        return () => {
            if (areaRef.current) {
                areaRef.current.removeEventListener("mouseenter", onMouseEnter);
                areaRef.current.removeEventListener("mouseleave", onMouseExit);
            }
        }
    }, [areaRef.current]);

    // React when the mouse is released
    useEffect(() => {        
        if (isHovered && dragContext.mouseReleased) {
            let result : DropResult = {
                data: {
                    ...dragContext // for now we'll just use the dragContext. Will make this more modular in the future
                }
            }
            config.onDrop(result);
            setResult(result);
        } else if (result != undefined) {
            setResult(undefined);
        }
    }, [dragContext.mouseReleased, isHovered]);

    return [areaRef, result];
};