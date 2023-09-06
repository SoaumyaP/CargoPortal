import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { NoteModel } from '../models/note.model';
import { HttpService } from '../services/http.service';

@Injectable()
export class NoteService {
    constructor(private httpService: HttpService) { }

    /** To add new note */
    createNote(note: NoteModel): Observable<NoteModel> {
        return this.httpService.create(`${environment.apiUrl}/notes`,
        note);
    }

    /** To update a note */
    updateNote(noteId: number, note: NoteModel): Observable<NoteModel> {
        return this.httpService.update(`${environment.apiUrl}/notes/${noteId}`,
        note);
    }

    /** To remove note */
    deleteNote(noteId: number) {
        return this.httpService.delete(`${environment.apiUrl}/notes/${noteId}`);
    }
}
