/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { TableCell } from '../models';
/**
 * @hidden
 */
export declare class ColorPaletteService {
    colorRows: string[][];
    setColorMatrix(palette: string[], columns: number): void;
    getCellCoordsFor(color: string): TableCell;
    getColorAt(cellCoords: TableCell): string;
    getNextCell(current: TableCell, horizontalStep: number, verticalStep: number): TableCell;
    private clampIndex;
}
