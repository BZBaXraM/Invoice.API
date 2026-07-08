import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ResponseModelData } from '../models/common.model';
import { ChatMessageDto, ChatRequest, ChatResponse } from '../models/chat.model';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/chat`;

  sendMessage(message: string, history: ChatMessageDto[]): Observable<ResponseModelData<ChatResponse>> {
    const request: ChatRequest = { message, history };
    return this.http.post<ResponseModelData<ChatResponse>>(this.baseUrl, request);
  }
}
