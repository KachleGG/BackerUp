import { Component, OnInit, signal, WritableSignal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { BackupJobsService } from '../../services/backup-jobs.service';
import { BackupJob } from '../../models/backup-job.model';

@Component({
  selector: 'app-backup-jobs',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './backup-jobs.html',
  styleUrl: './backup-jobs.scss',
})
export class BackupJobs implements OnInit {
  jobs: WritableSignal<BackupJob[]> = signal<BackupJob[]>([]);

  trackById(index: number, item: BackupJob): number {
    return item.id;
  }

  constructor(private backupJobsService: BackupJobsService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.backupJobsService.getAll().subscribe(data => this.jobs.set(data));
  }

  add(): void {
    this.router.navigate(['/backup-jobs/add']);
  }

  edit(id: number): void {
    this.router.navigate(['/backup-jobs/edit', id]);
  }

  delete(id: number): void {
    if (!confirm('Are you sure you want to delete this backup job?')) return;
    this.backupJobsService.delete(id).subscribe(() => this.jobs.update(list => list.filter(j => j.id !== id)));
  }
}

