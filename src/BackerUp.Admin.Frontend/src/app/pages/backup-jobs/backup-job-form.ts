import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BackupJobsService } from '../../services/backup-jobs.service';
import { BackupMethod, RetentionDto } from '../../models/backup-job.model';

@Component({
  selector: 'app-backup-job-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './backup-job-form.html',
  styleUrl: './backup-jobs.scss',
})
export class BackupJobForm implements OnInit {
  isEdit = false;
  id: number | null = null;

  method: BackupMethod = 'Full';
  timing = '';
  sources: string[] = [''];
  targets: string[] = [''];
  retention: RetentionDto = { count: 0, size: 0 };

  methods: BackupMethod[] = ['Full', 'Incremental', 'Differential'];

  constructor(
    private backupJobsService: BackupJobsService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.id = idParam ? Number(idParam) : null;
    this.isEdit = this.id != null;

    if (this.isEdit && this.id != null) {
      this.backupJobsService.getById(this.id).subscribe(j => {
        this.method = j.method;
        this.timing = j.timing;
        this.sources = j.sources.length ? [...j.sources] : [''];
        this.targets = j.targets.length ? [...j.targets] : [''];
        this.retention = j.retention ?? { count: 0, size: 0 };
        this.cdr.detectChanges();
      });
      return;
    }

    this.method = 'Full';
    this.timing = '';
    this.sources = [''];
    this.targets = [''];
    this.retention = { count: 0, size: 0 };
    this.cdr.detectChanges();
  }

  addSource(): void { this.sources.push(''); }
  removeSource(i: number): void { this.sources.splice(i, 1); }

  addTarget(): void { this.targets.push(''); }
  removeTarget(i: number): void { this.targets.splice(i, 1); }

  trackByIndex(index: number): number { return index; }

  hasMeaningfulSources(): boolean {
    return this.sources.some(source => source.trim().length > 0);
  }

  hasMeaningfulTargets(): boolean {
    return this.targets.some(target => target.trim().length > 0);
  }

  save(): void {
    const request = {
      // send method as enum name string (backend expects enum strings)
      method: this.method,
      timing: this.timing,
      sources: this.sources.filter(s => s.trim()),
      targets: this.targets.filter(t => t.trim()),
      retention: { ...this.retention }
    };

    if (this.isEdit && this.id != null) {
      this.backupJobsService.update(this.id, request).subscribe(() => this.router.navigate(['/backup-jobs']));
    } else {
      this.backupJobsService.create(request).subscribe(() => this.router.navigate(['/backup-jobs']));
    }
  }

  cancel(): void {
    this.router.navigate(['/backup-jobs']);
  }
}
