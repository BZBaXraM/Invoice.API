export interface ChatMessageDto {
  role: 'user' | 'assistant';
  content: string;
}

export interface ChatRequest {
  message: string;
  history: ChatMessageDto[];
}

export interface ChatResponse {
  reply: string;
  suggestions: string[];
  invoiceIds: string[];
}

export interface ChatDisplayMessage {
  role: 'user' | 'assistant';
  content: string;
  suggestions?: string[];
  invoiceIds?: string[];
}
