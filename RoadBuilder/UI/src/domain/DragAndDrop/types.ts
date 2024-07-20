export interface DropResult { 
    /**
     * The data from the drop operation
     */
    data?: any;

    /**
     * True if the area it was dropped in was the drop target, false otherwise.
     */
    isTarget?: boolean;
};