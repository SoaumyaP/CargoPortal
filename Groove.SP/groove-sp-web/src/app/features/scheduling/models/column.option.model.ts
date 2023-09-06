export interface ColumnOptionModel {
    /**
     * Column name. */
    name: string;

    /**Sequence of a column */
    sequence?: number;

    /**
     * True if column is already selected. */
    selected?: boolean;

    /**
     * To mark the column being dragged */
    dragging?: boolean;
}