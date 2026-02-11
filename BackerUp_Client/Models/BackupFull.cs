using Class_Lib;

namespace BackerUp_Client.Models {
    public class BackupFull : Backup {
        public override void PerformBackup(BackupJob job, JobsMetadata jobMeta) {
            // Full uses the base implementation
            base.PerformBackup(job, jobMeta);
        }
    }
}
