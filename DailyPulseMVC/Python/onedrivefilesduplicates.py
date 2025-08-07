
import os
import csv
from datetime import datetime

# Directory to search for duplicate files
search_directory = "/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/"
output_csv = "/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/FileNamesWithSizesPython/duplicaterecords.csv"

def get_file_info(file_path):
    """Get file information: last modified time, size, and filename."""
    try:
        stat = os.stat(file_path)
        last_modified_time = datetime.fromtimestamp(stat.st_mtime).strftime('%Y-%m-%d %H:%M:%S')
        size = stat.st_size
        filename = os.path.basename(file_path)
        return last_modified_time, size, filename
    except Exception as e:
        print(f"Error accessing file {file_path}: {e}")
        return None, None, None

def find_duplicates(directory):
    """Find duplicate files based on filename, last modified time, and size."""
    file_records = {}
    duplicates = []

    for root, _, files in os.walk(directory):
        for file in files:
            full_path = os.path.join(root, file)
            last_modified_time, size, filename = get_file_info(full_path)

            if last_modified_time is None:
                continue
            key = (filename, last_modified_time, size)
            if key in file_records:
                duplicates.append((full_path, *key))
            else:
                file_records[key] = full_path

    return duplicates

def write_to_csv(duplicates, output_file):
    """Write duplicate file details to a CSV file."""
    with open(output_file, mode='w', newline='') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow(["Full File Path", "Filename", "Last Modified Time", "Size (Bytes)"])
        for full_path, filename, last_modified_time, size in duplicates:
            if size >100000:  # Only include files larger than 100KB
                writer.writerow([full_path, filename, last_modified_time, size])

if __name__ == "__main__":
    duplicates = find_duplicates(search_directory)
    if duplicates:
        write_to_csv(duplicates, output_csv)
        print(f"Duplicate records written to {output_csv}")
    else:
        print("No duplicate files found.")
