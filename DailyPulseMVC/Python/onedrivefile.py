import os
import csv
from datetime import datetime

def list_files_and_save_to_csv(directory):
    # Get current timestamp for the output file name
    timestamp = datetime.now().strftime("%Y-%m-%d_%H_%M_%S")
    output_directory = "/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna/FileNamesWithSizesPython"
    os.makedirs(output_directory, exist_ok=True)  # Ensure the directory exists
    output_file = os.path.join(output_directory, f"allfilesNames_{timestamp}.csv")
    total_size_in_bytes = 0
    file_count = 0
    # Open the CSV file for writing
    with open(output_file, mode='w', newline='', encoding='utf-8') as csvfile:
        csv_writer = csv.writer(csvfile)
        # Write the header row
        csv_writer.writerow(["DatetimeCreated", "DatetimeModified", "Filename", "Foldername", "SizeInBytes", "FullFileName"])

        # Walk through the directory
        buffer = []
        for root, _, files in os.walk(directory):
            for file in files:
                file_path = os.path.join(root, file)
                try:
                    # Get file stats
                    stats = os.stat(file_path)
                    created_time = datetime.fromtimestamp(stats.st_ctime).strftime('%Y-%m-%d %H:%M:%S')
                    modified_time = datetime.fromtimestamp(stats.st_mtime).strftime('%Y-%m-%d %H:%M:%S')
                    size_in_bytes = stats.st_size
                    folder_name = os.path.basename(root)
                    # Update total size in bytes
                    total_size_in_bytes += stats.st_size
                    # Increment file count
                    file_count += 1
                    # Add file details to the buffer
                    buffer.append([created_time, modified_time, file, folder_name, size_in_bytes, file_path])

                    # Write to CSV if buffer reaches 70000 files
                    if len(buffer) >= 70000:
                        csv_writer.writerows(buffer)
                        buffer = []  # Clear the buffer
                except Exception as e:
                    print(f"Error processing file {file_path}: {e}")

        # Write remaining files in the buffer
        if buffer:
            csv_writer.writerows(buffer)

    print(f"File details saved to {output_file}")
        # Calculate total size in gigabytes
    total_size_gigabytes = round(total_size_in_bytes / (1024 ** 3), 4)

    # Path for the summary CSV file
    summary_file = os.path.join(output_directory, "allFilesSummary.csv")

    # Check if the file exists
    file_exists = os.path.isfile(summary_file)

    # Open the summary CSV file in append mode
    with open(summary_file, mode='a', newline='', encoding='utf-8') as summary_csvfile:
        summary_writer = csv.writer(summary_csvfile)
        # Write the header row if the file does not exist
        if not file_exists:
            summary_writer.writerow(["Datetime", "TotalSizeInBytes", "FileCount", "TotalSizeInGigabytes"])
        # Append the summary row
        summary_writer.writerow([datetime.now().strftime('%Y-%m-%d %H:%M:%S'), total_size_in_bytes, file_count, total_size_gigabytes])

    print(f"Summary details saved to {summary_file}")

# Directory to scan
directory_to_scan = "/Users/nagendra_subramanya@optum.com/Library/CloudStorage/OneDrive-Krishna"

# Call the function
list_files_and_save_to_csv(directory_to_scan)