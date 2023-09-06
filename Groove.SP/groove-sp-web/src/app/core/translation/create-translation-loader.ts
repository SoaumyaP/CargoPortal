
import { HttpService } from '../services/http.service';
import { CustomTranslationLoader } from './custom-translation-loader';

export function CreateTranslationLoader(httpService: HttpService) {
    return new CustomTranslationLoader(httpService);
}
