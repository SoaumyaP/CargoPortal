import { Injectable } from '@angular/core';
import { StringHelper } from '../helpers';

@Injectable({
    providedIn: 'root'
})

export class LocalStorageService {
    public static write(key: string, value: any) {
        if (value) {
            value = JSON.stringify(value);
        }
        localStorage.setItem(key, value);
    }

    public static read<T>(key: string): T {
        const value: string = localStorage.getItem(key);

        if (!StringHelper.isNullOrEmpty(value)) {
            return <T>JSON.parse(value);
        }

        return null;
    }

    public static remove(key: string) {
        localStorage.removeItem(key);
    }
}
