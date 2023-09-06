import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: 'breakOn' })
export class BreakOnPipe implements PipeTransform {
    transform(value: string, char: string) {
        let items = value.split(char).map(x => x.trim());
        return items.join(`${char}\n`);
    }
}