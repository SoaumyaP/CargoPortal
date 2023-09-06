import { Injectable } from '@angular/core';
import { StringHelper } from '../helpers';

@Injectable({
    providedIn: 'root'
})

export class CookieService {
    /*
    * General utils for managing cookies in Typescript.
    */
    public setCookie(name: string, value: string, expiredInMinutes?: number) {
        const date = new Date();
        let cookies = document.cookie?.split(';').map(c => c.trim());

        // // Set it expire in minutes
        if (expiredInMinutes > 0) {
            date.setTime(date.getTime() + (expiredInMinutes * 60 * 1000));
            const newExpires = `expires=${date.toString()}; path=/`;
            document.cookie = newExpires;
        } else {
            document.cookie = `expires=; path=/`;
        }

        if (!StringHelper.isNullOrEmpty(name)) {
            const newCookie = `${name}=${value}; path=/`;
            document.cookie = newCookie;
        }
    }

    public getCookie(name?: string) {
        const cookies = document.cookie?.split(';').map(c => c.trim());
        const expirationIndex = cookies.findIndex(c => c.startsWith('expires='));

        if (expirationIndex !== -1) {
            if (cookies[expirationIndex]) {
                const expirationTime = new Date(cookies[expirationIndex].split("=")[1]).getTime();

                if (expirationTime < new Date().getTime()) {
                    return null;
                } else {
                    return this.getCookieByName(name);
                }
            } else {
                return this.getCookieByName(name);
            }
        }
        else {
            return this.getCookieByName(name);
        }
    }

    private getCookieByName(name?: string) {
        const cookies = document.cookie?.split(';').map(c => c.trim());
        if (!name) {
            return document.cookie;
        }

        const cookieIndex = cookies.findIndex(c => c.startsWith(name));
        return cookieIndex !== -1 ? cookies[cookieIndex].split("=")[1] : null;
    }

    public deleteCookie(name: string) {
        const date = new Date();

        // Set it expire in -1 days
        date.setTime(date.getTime() + (-1 * 24 * 60 * 60 * 1000));

        document.cookie = name + "=; expires=" + date.toString() + "; path=/";
    }
}
