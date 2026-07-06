import { Component, computed, input, output } from '@angular/core';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-pagination',
  imports: [TranslatePipe],
  template: `
    <div class="pagination">
      <span>
        {{ 'pagination.range' | translate:{ start: rangeStart(), end: rangeEnd(), total: totalCount() } }}
      </span>
      <div class="pagination__controls">
        <button
          type="button"
          class="btn btn--secondary btn--sm"
          [disabled]="pageNumber() <= 1"
          (click)="pageChange.emit(pageNumber() - 1)"
        >
          {{ 'pagination.previous' | translate }}
        </button>
        <button
          type="button"
          class="btn btn--secondary btn--sm"
          [disabled]="pageNumber() >= totalPages()"
          (click)="pageChange.emit(pageNumber() + 1)"
        >
          {{ 'pagination.next' | translate }}
        </button>
      </div>
    </div>
  `,
})
export class PaginationComponent {
  readonly pageNumber = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly pageChange = output<number>();

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));
  readonly rangeStart = computed(() => (this.totalCount() === 0 ? 0 : (this.pageNumber() - 1) * this.pageSize() + 1));
  readonly rangeEnd = computed(() => Math.min(this.pageNumber() * this.pageSize(), this.totalCount()));
}
