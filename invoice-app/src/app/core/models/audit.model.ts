export interface AuditLogResponse {
  id: string;
  actorName: string;
  actorEmail?: string | null;
  entityType: string;
  entityId: string;
  action: 'Created' | 'Updated' | 'Deleted';
  changesJson: string;
  createdAt: string;
}

export interface AuditChangeEntry {
  property: string;
  old?: unknown;
  new?: unknown;
}

/** Parses the backend's ChangesJson ({ prop: { old, new } }) into a display-friendly list. */
export function parseAuditChanges(changesJson: string): AuditChangeEntry[] {
  try {
    const parsed = JSON.parse(changesJson) as Record<string, { old?: unknown; new?: unknown }>;
    return Object.entries(parsed).map(([property, values]) => ({
      property,
      old: values?.old,
      new: values?.new,
    }));
  } catch {
    return [];
  }
}
