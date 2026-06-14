import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClientsService } from '../../services/clients.service';
import { BackupJobsService } from '../../services/backup-jobs.service';
import { BackupJob } from '../../models/backup-job.model';

@Component({
  selector: 'app-client-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './client-form.html',
  styleUrl: './clients.scss',
})
export class ClientForm implements OnInit {
  isEdit = false;
  id: string | null = null;
  jobsLoading = true;

  name = '';
  isActive = true;
  jobs: BackupJob[] = [];
  selectedJobIds: number[] = [];

  constructor(
    private clientsService: ClientsService,
    private backupJobsService: BackupJobsService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    this.isEdit = !!this.id;

    if (this.isEdit && this.id) {
      this.clientsService.getById(this.id).subscribe(c => {
        this.name = c.name;
        this.isActive = c.isActive;
        this.selectedJobIds = c.jobIds ?? [];
        this.cdr.detectChanges();
      });
    }

    this.loadJobs();
  }

  private loadJobs(): void {
    this.jobsLoading = true;
    this.backupJobsService.getAll().subscribe((jobs: BackupJob[]) => {
      this.jobs = jobs;
      this.jobsLoading = false;
      this.cdr.detectChanges();
    });
  }

  isSelected(jobId: number): boolean {
    return this.selectedJobIds.includes(jobId);
  }

  toggleJob(jobId: number): void {
    if (this.isSelected(jobId)) {
      this.selectedJobIds = this.selectedJobIds.filter(id => id !== jobId);
      return;
    }

    this.selectedJobIds = [...this.selectedJobIds, jobId];
  }

  clearJobs(): void {
    this.selectedJobIds = [];
  }

  get selectedJobs(): BackupJob[] {
    return this.jobs.filter(job => this.selectedJobIds.includes(job.id));
  }

  save(): void {
    const request = { name: this.name, isActive: this.isActive, jobIds: this.selectedJobIds };
    if (this.isEdit && this.id) {
      this.clientsService.update(this.id, request).subscribe(() => this.router.navigate(['/clients']));
    } else {
      this.clientsService.create(request).subscribe(() => this.router.navigate(['/clients']));
    }
  }

  cancel(): void {
    this.router.navigate(['/clients']);
  }
}
