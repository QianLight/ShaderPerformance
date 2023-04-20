using System.Collections.Generic;


namespace TDTools.ResourceScanner {

    public class ScannerResult {
        public HashSet<string> skillFiles;
        public HashSet<string> animationFiles;
        public HashSet<string> FxFiles;
        public HashSet<string> TimelineFiles;
        public HashSet<string> BehitFiles;
        public List<string> TimelineToScan;
        public HashSet<string> MissingFiles;

        public List<string> AllFiles {
            get {
                List<string> result = new List<string>();
                result.AddRange(skillFiles);
                result.AddRange(animationFiles);
                result.AddRange(FxFiles);
                result.AddRange(TimelineFiles);
                result.AddRange(BehitFiles);
                return result;
            }
        }

        public ScannerResult() {
            skillFiles = new HashSet<string>();
            animationFiles = new HashSet<string>();
            FxFiles = new HashSet<string>();
            TimelineFiles = new HashSet<string>();
            BehitFiles = new HashSet<string>();
            TimelineToScan = new List<string>();
            MissingFiles = new HashSet<string>();
        }

        public void AddRange(ScannerResult other) {
            skillFiles.UnionWith(other.skillFiles);
            animationFiles.UnionWith(other.animationFiles);
            FxFiles.UnionWith(other.FxFiles);
            TimelineFiles.UnionWith(other.TimelineFiles);
            BehitFiles.UnionWith(other.BehitFiles);
            MissingFiles.UnionWith(other.MissingFiles);

            TimelineToScan.AddRange(other.TimelineToScan);
        }

        public void Difference(ScannerResult other) {
            skillFiles.ExceptWith(other.skillFiles);
            animationFiles.ExceptWith(other.animationFiles);
            FxFiles.ExceptWith(other.FxFiles);
            TimelineFiles.ExceptWith(other.TimelineFiles);
            BehitFiles.ExceptWith(other.BehitFiles);
        }
    }

}
