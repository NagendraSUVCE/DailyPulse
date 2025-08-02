from selenium import webdriver
from selenium.webdriver.common.by import By
import time

# Set up the driver
driver = webdriver.Chrome()

# Open a website
driver.get("https://www.google.com")

# Find the search box and type something
# search_box = driver.find_element(By.NAME, "q")
# search_box.send_keys("Hello from Selenium!")

# Submit the search
# search_box.submit()


# Wait for the page to load (optional but recommended)
time.sleep(20)

# Find the button using aria-label and click it
sign_in_button = driver.find_element(By.CSS_SELECTOR, '[aria-label="Sign in"]')
sign_in_button.click()

# Optional: wait and close
time.sleep(3)
driver.find_element(By.CSS_SELECTOR, '[aria-label="Email or phone"]').send_keys("nagendrasubramanya@gmail.com")

time.sleep(3)

next_button = driver.find_element(By.XPATH, '//span[@jsname="V67aGc" and text()="Next"]')
next_button.click()

# Wait for 25 seconds
time.sleep(55)
# Close the browser
driver.quit()

